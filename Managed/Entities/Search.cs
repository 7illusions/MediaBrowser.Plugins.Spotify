using MediaBrowser.Plugins.Spotify.Managed.Wrapper;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MediaBrowser.Plugins.Spotify.Managed.Entities
{

    public class Search : IDisposable
    {
        private bool _disposed;
        private IntPtr _searchPtr;

        public delegate void Search_Completed(IntPtr result, IntPtr userDataPtr);

        public bool IsLoaded { get; private set; }

        public List<Album> Albums { get; private set; }
        public List<Artist> Artists { get; private set; }
        public List<Track> Tracks { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {

            dispose(true);
            GC.SuppressFinalize(this);

        }

        ~Search()
        {

            dispose(false);

        }

        private void dispose(bool disposing)
        {

            if (!_disposed)
            {

                if (disposing)
                {

                    safeReleaseToplist();

                }

                _disposed = true;

            }

        }

        #endregion

        public static Search BeginSearch(string query)
        {
            try
            {
                Search search = new Search();

                Search_Completed callback = new Search_Completed(search.Search_Complete_Callback);
                IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callback);

                search._searchPtr = libspotify.sp_search_create(Session.GetSessionPtr(), query, 0, 20, 0, 20, 0, 20, 0, 20, sp_search_type.SP_SEARCH_STANDARD, callbackPtr, IntPtr.Zero);
                return search;

            }
            catch (Exception)
            {
                return null;
            }
        }

        private void Search_Complete_Callback(IntPtr result, IntPtr userDataPtr)
        {
            try
            {
                libspotify.sp_error error = libspotify.sp_search_error(result);

                if (error != libspotify.sp_error.OK)
                {
                    return;
                }

                var albumCount = libspotify.sp_search_num_albums(_searchPtr);
                var artistCount = libspotify.sp_search_num_artists(_searchPtr);
                var trackCount = libspotify.sp_search_num_tracks(_searchPtr);

                List<Album> albums = new List<Album>();
                for (int i = 0; i < albumCount; i++)
                {
                    var ptr = libspotify.sp_search_album(_searchPtr, i);
                    albums.Add(new Album(ptr));
                }

                List<Artist> artists = new List<Artist>();
                for (int i = 0; i < artistCount; i++)
                {
                    var ptr = libspotify.sp_search_artist(_searchPtr, i);
                    artists.Add(new Artist(ptr));
                }

                List<Track> tracks = new List<Track>();
                for (int i = 0; i < trackCount; i++)
                {
                    var ptr = libspotify.sp_search_track(_searchPtr, i);
                    tracks.Add(new Track(ptr));
                }

                this.Albums = albums;
                this.Artists = artists;
                this.Tracks = tracks;
                this.IsLoaded = true;
            }
            finally
            {
                safeReleaseToplist();
            }
        }

        private void safeReleaseToplist()
        {
            if (_searchPtr != IntPtr.Zero)
            {
                try
                {
                    libspotify.sp_toplistbrowse_release(_searchPtr);
                }
                catch
                { }
            }
        }
    }
}
