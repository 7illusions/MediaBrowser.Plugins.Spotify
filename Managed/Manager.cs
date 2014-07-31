using MediaBrowser.Plugins.Spotify.Managed.Entities;
using MediaBrowser.Plugins.Spotify.Managed.Wrapper;
using MediaBrowser.Plugins.Spotify.Manged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.Spotify.Managed
{
    public class Manager
    {

        private delegate bool Test();
        public delegate void MainThreadMessageDelegate(object[] args);

        private static AutoResetEvent _programSignal;
        private static AutoResetEvent _mainSignal;
        private static Queue<MainThreadMessage> _mq = new Queue<MainThreadMessage>();
        private static bool _shutDown = false;
        private static object _syncObj = new object();
        private static object _initSync = new object();
        private static bool _initted = false;
        private static bool _isRunning = false;
        private static Action<IntPtr> d_notify = new Action<IntPtr>(Session_OnNotifyMainThread);
        private static Action<IntPtr> d_on_logged_in = new Action<IntPtr>(Session_OnLoggedIn);
        private static Thread _t;

        private static readonly int REQUEST_TIMEOUT = 30;

        private class MainThreadMessage
        {
            public MainThreadMessageDelegate d;
            public object[] payload;
        }

        public static bool IsRunning
        {

            get { return _isRunning; }

        }
        public static bool Login(byte[] appkey, string username, string password)
        {

            postMessage(Session.Login, new object[] { appkey, username, password });

            _programSignal.WaitOne();

            if (Session.LoginError != libspotify.sp_error.OK)
            {

                //TODO Log.Error(Plugin.LOG_MODULE, "Login failed: {0}", libspotify.sp_error_message(Session.LoginError));
                return false;
            }

            return true;

        }

        public static void Initialize()
        {

            if (_initted)
                return;

            lock (_initSync)
            {

                try
                {

                    Session.OnNotifyMainThread += d_notify;
                    Session.OnLoggedIn += d_on_logged_in;

                    _programSignal = new AutoResetEvent(false);

                    _t = new Thread(new ThreadStart(mainThread));
                    _t.Start();

                    _programSignal.WaitOne();

                    //TODO Log.Debug(Plugin.LOG_MODULE, "Main thread running...");

                    _initted = true;

                }
                catch
                {

                    Session.OnNotifyMainThread -= d_notify;
                    Session.OnLoggedIn -= d_on_logged_in;

                    if (_t != null)
                    {

                        try
                        {

                            _t.Abort();

                        }
                        catch { }
                        finally
                        {

                            _t = null;

                        }

                    }

                }

            }

        }

        public static int GetUserCountry()
        {

            return Session.GetUserCountry();

        }

        #region Playlist

        public static List<PlaylistManager.PlaylistInfo> GetAllSessionPlaylists()
        {

            waitFor(delegate
            {
                return PlaylistManager.Instance.IsLoaded
                    && PlaylistManager.Instance.PlaylistsAreLoaded;
            }, REQUEST_TIMEOUT);

            return PlaylistManager.Instance.GetAllPlaylists();

        }     

        public static Playlist GetPlaylist(IntPtr playlistPtr, bool needTracks)
        {

            Playlist playlist = Playlist.Get(playlistPtr);

            if (playlist == null)
                return null;

            bool success = waitFor(delegate
            {
                return playlist.IsLoaded && needTracks ? playlist.TracksAreLoaded : true;
            }, REQUEST_TIMEOUT);

            return playlist;

        }

        public static Playlist GetInboxPlaylist()
        {

            IntPtr inboxPtr = IntPtr.Zero;

            try
            {

                inboxPtr = libspotify.sp_session_inbox_create(Session.GetSessionPtr());

                Playlist p = Playlist.Get(inboxPtr);

                bool success = waitFor(delegate
                {
                    return p.IsLoaded;
                }, REQUEST_TIMEOUT);

                return p;

            }
            finally
            {

                try
                {

                    if (inboxPtr != IntPtr.Zero)
                        libspotify.sp_playlist_release(inboxPtr);

                }
                catch { }

            }

        }

        public static Playlist GetStarredPlaylist()
        {

            IntPtr starredPtr = IntPtr.Zero;

            try
            {

                starredPtr = libspotify.sp_session_starred_create(Session.GetSessionPtr());

                Playlist p = Playlist.Get(starredPtr);

                bool success = waitFor(delegate
                {
                    return p.IsLoaded;
                }, REQUEST_TIMEOUT);

                return p;

            }
            finally
            {

                try
                {

                    if (starredPtr != IntPtr.Zero)
                        libspotify.sp_playlist_release(starredPtr);

                }
                catch { }

            }

        }

        #endregion

        #region Search

        public static Search Search(string data)
        {
            var search = Entities.Search.BeginSearch(data);            

            bool success = waitFor(delegate
            {
                return search.IsLoaded;
            }, REQUEST_TIMEOUT);

            return search;

        }

        #endregion

        public static TopList GetToplist(string data)
        {

            string[] parts = data.Split("|".ToCharArray());

            int region = parts[0].Equals("ForMe") ? (int)libspotify.sp_toplistregion.SP_TOPLIST_REGION_USER : parts[0].Equals("Everywhere") ? (int)libspotify.sp_toplistregion.SP_TOPLIST_REGION_EVERYWHERE : Convert.ToInt32(parts[0]);
            libspotify.sp_toplisttype type = parts[1].Equals("Artists") ? libspotify.sp_toplisttype.SP_TOPLIST_TYPE_ARTISTS : parts[1].Equals("Albums") ? libspotify.sp_toplisttype.SP_TOPLIST_TYPE_ALBUMS : libspotify.sp_toplisttype.SP_TOPLIST_TYPE_TRACKS;

            TopList toplist = TopList.BeginBrowse(type, region);

            bool success = waitFor(delegate
            {
                return toplist.IsLoaded;
            }, REQUEST_TIMEOUT);

            return toplist;

        }

        public static byte[] GetAlbumArt(IntPtr albumPtr)
        {

            IntPtr coverPtr = libspotify.sp_album_cover(albumPtr, libspotify.sp_image_size.SP_IMAGE_SIZE_LARGE);

            // NOTE: in API 10 sp_image_is_loaded() always returns true despite empty byte buffer, so using
            // callbacks now to determine when loaded.  Not sure how this will behave with cached images...

            using (Image img = Image.Load(libspotify.sp_image_create(Session.GetSessionPtr(), coverPtr)))
            {

                waitFor(delegate()
                {
                    return img.IsLoaded;
                }, REQUEST_TIMEOUT);

                int bytes = 0;
                IntPtr bufferPtr = libspotify.sp_image_data(img.ImagePtr, out bytes);
                byte[] buffer = new byte[bytes];
                Marshal.Copy(bufferPtr, buffer, 0, buffer.Length);

                Console.WriteLine("{0}, {1}", buffer.Length, libspotify.sp_image_is_loaded(img.ImagePtr));

                return buffer;
            }

        }

        public static IntPtr[] GetAlbumTracks(IntPtr albumPtr)
        {

            using (Album album = new Album(albumPtr))
            {

                if (!waitFor(delegate
                {
                    return libspotify.sp_album_is_loaded(album.AlbumPtr);
                }, REQUEST_TIMEOUT))
                    Console.WriteLine("GetAlbumTracks() TIMEOUT waiting for album to load");

                if (album.BeginBrowse())
                {

                    if (!waitFor(delegate()
                    {
                        return album.IsBrowseComplete;
                    }, REQUEST_TIMEOUT))
                        Console.WriteLine("GetAlbumTracks() TIMEOUT waiting for browse to complete");

                }

                if (album.TrackPointers == null)
                    return null;

                return album.TrackPointers.ToArray();

            }

        }

        public static IntPtr[] GetArtistAlbums(IntPtr artistPtr)
        {

            using (Artist artist = new Artist(artistPtr))
            {

                if (!waitFor(delegate
                {
                    return libspotify.sp_artist_is_loaded(artist.ArtistPtr);
                }, REQUEST_TIMEOUT))
                    Console.WriteLine("GetArtistAlbums() TIMEOUT waiting for artist to load");

                if (artist.BeginBrowse())
                {

                    if (!waitFor(delegate()
                    {
                        return artist.IsBrowseComplete;
                    }, REQUEST_TIMEOUT))
                        Console.WriteLine("GetArtistAlbums() TIMEOUT waiting for browse to complete");

                }

                if (artist.AlbumPointers == null)
                    return null;

                return artist.AlbumPointers.ToArray();

            }

        }

        public static PlaylistManager GetUserPlaylists(IntPtr userPtr)
        {

            IntPtr ptr = IntPtr.Zero;

            try
            {

                ptr = libspotify.sp_session_publishedcontainer_for_user_create(Session.GetSessionPtr(), GetUserCanonicalNamePtr(userPtr));

                PlaylistManager c = PlaylistManager.Get(ptr);

                waitFor(delegate
                {
                    return c.IsLoaded
                        && c.PlaylistsAreLoaded;
                }, REQUEST_TIMEOUT);

                return c;

            }
            finally
            {

                //try {

                //    if (ptr != IntPtr.Zero)
                //        libspotify.sp_playlistcontainer_release(ptr);

                //} catch { }

            }

        }

        public static IntPtr GetUserCanonicalNamePtr(IntPtr userPtr)
        {

            waitFor(delegate()
            {
                return libspotify.sp_user_is_loaded(userPtr);
            }, REQUEST_TIMEOUT);

            return libspotify.sp_user_canonical_name(userPtr);

        }

        public static string GetUserDisplayName(IntPtr userPtr)
        {

            waitFor(delegate()
            {
                return libspotify.sp_user_is_loaded(userPtr);
            }, REQUEST_TIMEOUT);

            return Functions.PtrToString(libspotify.sp_user_display_name(userPtr));

        }

        private static bool waitFor(Test t, int timeout)
        {

            DateTime start = DateTime.Now;

            while (DateTime.Now.Subtract(start).Seconds < timeout)
            {

                if (t.Invoke())
                {

                    return true;

                }

                Thread.Sleep(10);

            }

            return false;

        }

        public static void ShutDown()
        {

            lock (_syncObj)
            {

                libspotify.sp_session_player_unload(Session.GetSessionPtr());
                libspotify.sp_session_logout(Session.GetSessionPtr());

                try
                {

                    if (PlaylistManager.Instance != null)
                    {

                        PlaylistManager.Instance.Dispose();

                    }

                }
                catch { }

                if (_mainSignal != null)
                    _mainSignal.Set();
                _shutDown = true;

            }

            _programSignal.WaitOne(2000, false);

        }

        private static void mainThread()
        {

            try
            {

                _mainSignal = new AutoResetEvent(false);

                int timeout = Timeout.Infinite;
                DateTime lastEvents = DateTime.MinValue;

                _isRunning = true;
                _programSignal.Set(); // this signals to program thread that loop is running   

                while (true)
                {

                    if (_shutDown)
                        break;

                    _mainSignal.WaitOne(timeout, false);

                    if (_shutDown)
                        break;

                    lock (_syncObj)
                    {

                        try
                        {

                            if (Session.GetSessionPtr() != IntPtr.Zero)
                            {

                                do
                                {

                                    libspotify.sp_session_process_events(Session.GetSessionPtr(), out timeout);

                                } while (timeout == 0);

                            }

                        }
                        catch (Exception ex)
                        {

                            //Log.Debug(Plugin.LOG_MODULE, "Exception invoking sp_session_process_events", ex);

                        }

                        while (_mq.Count > 0)
                        {

                            MainThreadMessage m = _mq.Dequeue();
                            m.d.Invoke(m.payload);

                        }

                    }

                }

            }
            catch (Exception ex)
            {

                //Log.Error(Plugin.LOG_MODULE, "mainThread() unhandled exception", ex);

            }
            finally
            {

                _isRunning = false;
                if (_programSignal != null)
                    _programSignal.Set();

            }

        }

        public static void Session_OnLoggedIn(IntPtr obj)
        {

            _programSignal.Set();

        }

        public static void Session_OnNotifyMainThread(IntPtr sessionPtr)
        {

            _mainSignal.Set();

        }

        private static void postMessage(MainThreadMessageDelegate d, object[] payload)
        {

            _mq.Enqueue(new MainThreadMessage() { d = d, payload = payload });

            lock (_syncObj)
            {

                _mainSignal.Set();

            }

        }

    }
}
