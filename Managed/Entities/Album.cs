
using MediaBrowser.Plugins.Spotify.Managed.Wrapper;
using MediaBrowser.Plugins.Spotify.Manged;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MediaBrowser.Plugins.Spotify.Managed.Entities
{
    public class Album : IDisposable
    {
        private bool _disposed;
        private IntPtr _browsePtr;
        private AlbumBrowseCompleted _albumBrowseCompleted;

        public string Name { get; private set; }
        public IntPtr AlbumPtr { get; private set; }
        public string Artist { get; private set; }
        public bool IsBrowseComplete { get; private set; }
        public List<IntPtr> TrackPointers { get; private set; }
        public libspotify.sp_albumtype Type { get; private set; }

        public Album(IntPtr albumPtr)
        {
            if (albumPtr == IntPtr.Zero)
                throw new ArgumentNullException("Album pointer is null.");

            this.AlbumPtr = albumPtr;
            this.Name = Functions.PtrToString(libspotify.sp_album_name(albumPtr));
            this.Type = libspotify.sp_album_type(albumPtr);
            IntPtr artistPtr = libspotify.sp_album_artist(albumPtr);
            if (artistPtr != IntPtr.Zero)
                this.Artist = Functions.PtrToString(libspotify.sp_artist_name(artistPtr));

        }

        #region IDisposable Members

        public void Dispose()
        {

            dispose(true);
            GC.SuppressFinalize(this);

        }

        ~Album()
        {

            dispose(false);

        }

        private void dispose(bool disposing)
        {

            if (!_disposed)
            {

                if (disposing)
                {

                    SafeReleaseAlbum();

                }

                _disposed = true;

            }

        }

        #endregion

        private void SafeReleaseAlbum()
        {

            if (this._browsePtr != IntPtr.Zero)
            {

                try
                {                    
                    libspotify.sp_albumbrowse_release(_browsePtr);
                }
                catch { }

            }

        }

        private void Albumbrowse_Completed(IntPtr result, IntPtr userDataPtr)
        {

            try
            {

                libspotify.sp_error error = libspotify.sp_albumbrowse_error(result);

                if (error != libspotify.sp_error.OK)
                {
                    return;
                }

                int numtracks = libspotify.sp_albumbrowse_num_tracks(_browsePtr);

                List<IntPtr> trackPtrs = new List<IntPtr>();

                for (int i = 0; i < libspotify.sp_albumbrowse_num_tracks(_browsePtr); i++)
                {

                    trackPtrs.Add(libspotify.sp_albumbrowse_track(_browsePtr, i));

                }

                this.TrackPointers = trackPtrs;

                this.IsBrowseComplete = true;

            }
            finally
            {
                SafeReleaseAlbum();
            }

        }

        public bool BeginBrowse()
        {
            try
            {
                _albumBrowseCompleted = new AlbumBrowseCompleted(this.Albumbrowse_Completed);
                IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(_albumBrowseCompleted);
                _browsePtr = libspotify.sp_albumbrowse_create(Session.GetSessionPtr(), this.AlbumPtr, callbackPtr, IntPtr.Zero);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

    }
}
