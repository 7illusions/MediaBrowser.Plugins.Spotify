
using MediaBrowser.Plugins.Spotify.Managed.Entities;
using MediaBrowser.Plugins.Spotify.Managed.Wrapper;
using MediaBrowser.Plugins.Spotify.Manged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.Spotify.Managed
{
    public class PlaylistManager : IDisposable
    {
        public class PlaylistInfo
        {
            public IntPtr ManagerPointer;
            public IntPtr Pointer;
            public ulong FolderID;
            public libspotify.sp_playlist_type PlaylistType;
            public string Name = string.Empty;            
            public PlaylistInfo Parent;
            public List<PlaylistInfo> Children = new List<PlaylistInfo>();
        }

        private IntPtr _containerPtr;
        private bool _disposed;

        private static PlaylistManager _sessionManager;        

        private PlaylistManager(IntPtr containerPtr)
        {
            _containerPtr = containerPtr;
        }

        #region IDisposable Members

        public void Dispose()
        {

            dispose(true);
            GC.SuppressFinalize(this);

        }

        ~PlaylistManager()
        {

            dispose(false);

        }

        private void dispose(bool disposing)
        {

            if (!_disposed)
            {              
                _disposed = true;
            }

        }

        #endregion

        public static PlaylistManager Instance
        {
            get
            {
                if (_sessionManager == null)
                {

                    if (Session.GetSessionPtr() == IntPtr.Zero)
                        throw new InvalidOperationException("No valid session.");

                    _sessionManager = new PlaylistManager(libspotify.sp_session_playlistcontainer(Session.GetSessionPtr()));

                }

                return _sessionManager;
            }

        }

        public static PlaylistManager Get(IntPtr containerPtr)
        {

            return new PlaylistManager(containerPtr);

        }    

        public bool IsLoaded
        {

            get
            {

                return libspotify.sp_playlistcontainer_is_loaded(_containerPtr);

            }

        }

        public bool PlaylistsAreLoaded
        {

            get
            {

                if (!this.IsLoaded)
                    return false;

                int count = libspotify.sp_playlistcontainer_num_playlists(_containerPtr);

                for (int i = 0; i < count; i++)
                {

                    if (libspotify.sp_playlistcontainer_playlist_type(_containerPtr, i) == libspotify.sp_playlist_type.SP_PLAYLIST_TYPE_PLAYLIST)
                    {
                        using (Playlist p = Playlist.Get(libspotify.sp_playlistcontainer_playlist(_containerPtr, i)))
                        {

                            if (!p.IsLoaded)
                                return false;

                        }
                    }

                }

                return true;

            }

        }

        public List<PlaylistInfo> GetAllPlaylists()
        {

            if (!Instance.IsLoaded)
                throw new InvalidOperationException("Container is not loaded.");

            List<PlaylistInfo> playlists = new List<PlaylistInfo>();

            for (int i = 0; i < libspotify.sp_playlistcontainer_num_playlists(_containerPtr); i++)
            {

                if (libspotify.sp_playlistcontainer_playlist_type(_containerPtr, i) == libspotify.sp_playlist_type.SP_PLAYLIST_TYPE_PLAYLIST)
                {

                    IntPtr playlistPtr = libspotify.sp_playlistcontainer_playlist(_containerPtr, i);

                    playlists.Add(new PlaylistInfo()
                    {
                        Pointer = playlistPtr,
                        PlaylistType = libspotify.sp_playlist_type.SP_PLAYLIST_TYPE_PLAYLIST,
                        ManagerPointer = _containerPtr,
                        Name = Functions.PtrToString(libspotify.sp_playlist_name(playlistPtr))
                    });

                }

            }

            return playlists;

        }
       
        public List<PlaylistInfo> GetChildren(PlaylistInfo info)
        {

            if (!Instance.IsLoaded)
                throw new InvalidOperationException("Container is not loaded.");

            PlaylistInfo tree = BuildTree();

            if (info == null)
            {

                return tree.Children;

            }
            else
            {

                return SearchTreeRecursive(tree, info).Children;

            }

        }

        private PlaylistInfo SearchTreeRecursive(PlaylistInfo tree, PlaylistInfo find)
        {

            if (tree.FolderID == find.FolderID)
                return tree;

            foreach (PlaylistInfo playlist in tree.Children)
            {

                if (playlist.PlaylistType == libspotify.sp_playlist_type.SP_PLAYLIST_TYPE_START_FOLDER)
                {

                    if (playlist.FolderID == find.FolderID)
                        return playlist;

                    PlaylistInfo p2 = SearchTreeRecursive(playlist, find);

                    if (p2 != null)
                        return p2;

                }

            }

            return null;

        }

        private PlaylistInfo BuildTree()
        {

            PlaylistInfo current = new PlaylistInfo();
            current.FolderID = ulong.MaxValue; //root

            for (int i = 0; i < libspotify.sp_playlistcontainer_num_playlists(_containerPtr); i++)
            {

                PlaylistInfo playlist = new PlaylistInfo()
                {
                    PlaylistType = libspotify.sp_playlistcontainer_playlist_type(_containerPtr, i),
                    ManagerPointer = _containerPtr
                };

                switch (playlist.PlaylistType)
                {

                    case libspotify.sp_playlist_type.SP_PLAYLIST_TYPE_START_FOLDER:

                        playlist.FolderID = libspotify.sp_playlistcontainer_playlist_folder_id(_containerPtr, i);
                        playlist.Name = GetFolderName(_containerPtr, i);
                        playlist.Parent = current;
                        current.Children.Add(playlist);
                        current = playlist;

                        break;

                    case libspotify.sp_playlist_type.SP_PLAYLIST_TYPE_END_FOLDER:

                        current = current.Parent;
                        break;

                    case libspotify.sp_playlist_type.SP_PLAYLIST_TYPE_PLAYLIST:

                        playlist.Pointer = libspotify.sp_playlistcontainer_playlist(_containerPtr, i);
                        playlist.Parent = current;
                        current.Children.Add(playlist);

                        break;

                }


            }

            while (current.Parent != null)
            {

                current = current.Parent;

            }

            return current;

        }

        private string GetFolderName(IntPtr containerPtr, int index)
        {

            IntPtr namePtr = Marshal.AllocHGlobal(128);

            try
            {

                libspotify.sp_error error = libspotify.sp_playlistcontainer_playlist_folder_name(containerPtr, index, namePtr, 128);

                return Functions.PtrToString(namePtr);

            }
            finally
            {

                Marshal.FreeHGlobal(namePtr);

            }

        } 


    }
}
