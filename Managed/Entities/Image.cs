
using MediaBrowser.Plugins.Spotify.Managed.Wrapper;
using System;
using System.Runtime.InteropServices;

namespace MediaBrowser.Plugins.Spotify.Managed.Entities
{
    internal class Image : IDisposable
    {
        private Image_Loaded_Delegate _d;
        private IntPtr _callbackPtr;
        private bool _disposed;

        internal bool IsLoaded { get; private set; }
        internal IntPtr ImagePtr { get; private set; }

        internal static Image Load(IntPtr ptr)
        {
            Image image = new Image();
            image.ImagePtr = ptr;
            image.beginLoad();

            return image;
        }

        private Image() { }

        #region IDisposable Members

        public void Dispose()
        {

            dispose(true);
            GC.SuppressFinalize(this);

        }

        ~Image()
        {

            dispose(false);

        }

        private void dispose(bool disposing)
        {

            if (!_disposed)
            {

                if (disposing)
                {

                    safeReleaseImage();

                }

                _disposed = true;

            }

        }

        #endregion

        private void safeReleaseImage()
        {
            if (this.ImagePtr != IntPtr.Zero)
            {
                try
                {
                    libspotify.sp_image_release(this.ImagePtr);
                }
                catch { }
            }
        }

        private void beginLoad()
        {

            _d = new Image_Loaded_Delegate(onImageLoaded);
            _callbackPtr = Marshal.GetFunctionPointerForDelegate(_d);
            libspotify.sp_image_add_load_callback(ImagePtr, _callbackPtr, IntPtr.Zero);
        }

        private void onImageLoaded(IntPtr imagePtr, IntPtr userDataPtr)
        {
            libspotify.sp_image_remove_load_callback(this.ImagePtr, _callbackPtr, IntPtr.Zero);
            this.IsLoaded = true;
        }
    }
}
