using MediaBrowser.Plugins.Spotify.Managed.Wrapper;
using MediaBrowser.Plugins.Spotify.Manged;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace MediaBrowser.Plugins.Spotify.Managed.Entities
{
    public class Artist : IDisposable
    {

        private bool _disposed;
        private IntPtr _browsePointer;
        private ArtistbrowseCompleted _artistBrowseCompleted;

        public IntPtr ArtistPtr { get; private set; }
        public string Name { get; private set; }
        public bool IsBrowseComplete { get; private set; }
        public List<IntPtr> AlbumPointers { get; private set; }

        public Artist(IntPtr artistPtr)
        {

            if (artistPtr == IntPtr.Zero)
                throw new ArgumentNullException("Artist pointer is null.");

            this.ArtistPtr = artistPtr;
            this.Name = Functions.PtrToString(libspotify.sp_artist_name(artistPtr));

        }

        #region IDisposable Members

        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Artist()
        {
            dispose(false);
        }

        private void dispose(bool disposing)
        {
            if (!_disposed)
            {

                if (disposing)
                {

                    SafeReleaseArtist();

                }

                _disposed = true;
            }
        }

        #endregion

        private void SafeReleaseArtist()
        {
            if (_browsePointer != IntPtr.Zero)
            {

                try
                {
                    libspotify.sp_artistbrowse_release(_browsePointer);
                }
                catch { }
            }

            if (ArtistPtr != IntPtr.Zero)
            {

                try
                {
                    libspotify.sp_artist_release(ArtistPtr);
                }
                catch { }
            }
        }

        private void Artistbrowse_Completed(IntPtr result, IntPtr userDataPtr)
        {
            try
            {
                libspotify.sp_error error = libspotify.sp_artistbrowse_error(result);

                if (error != libspotify.sp_error.OK)
                {
                    return;
                }

                int numalbums = libspotify.sp_artistbrowse_num_albums(_browsePointer);

                List<IntPtr> albumPtrs = new List<IntPtr>();

                for (int i = 0; i < libspotify.sp_artistbrowse_num_albums(_browsePointer); i++)
                {
                    IntPtr albumPtr = libspotify.sp_artistbrowse_album(_browsePointer, i);

                    // excluding singles, compilations, and unknowns
                    if (libspotify.sp_album_type(albumPtr) == libspotify.sp_albumtype.SP_ALBUMTYPE_ALBUM
                        && libspotify.sp_album_is_available(albumPtr))
                        albumPtrs.Add(albumPtr);
                }

                this.AlbumPointers = albumPtrs;

                this.IsBrowseComplete = true;

            }
            finally
            {

                SafeReleaseArtist();

            }

        }

        public bool BeginBrowse()
        {
            try
            {
                _artistBrowseCompleted = new ArtistbrowseCompleted(this.Artistbrowse_Completed);
                IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(_artistBrowseCompleted);
                _browsePointer = libspotify.sp_artistbrowse_create(Session.GetSessionPtr(), this.ArtistPtr, libspotify.sp_artistbrowse_type.SP_ARTISTBROWSE_NO_TRACKS, callbackPtr, IntPtr.Zero);

                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
