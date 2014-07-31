using MediaBrowser.Plugins.Spotify.Managed.Wrapper;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaBrowser.Plugins.Spotify.Managed.Entities
{

    public class TopList : IDisposable
    {

        private bool _disposed;
        private IntPtr _browsePtr;

        public delegate void Toplistbrowse_Completed(IntPtr result, IntPtr userDataPtr);

        public bool IsLoaded { get; private set; }
        public List<IntPtr> Ptrs { get; private set; }
        public libspotify.sp_toplisttype ToplistType { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {

            dispose(true);
            GC.SuppressFinalize(this);

        }

        ~TopList()
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

        public static TopList BeginBrowse(libspotify.sp_toplisttype type, int region)
        {
            try
            {
                TopList t = new TopList();
                t.ToplistType = type;
                Toplistbrowse_Completed d = new Toplistbrowse_Completed(t.toplistbrowse_complete);
                IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(d);

                t._browsePtr = libspotify.sp_toplistbrowse_create(Session.GetSessionPtr(), type, region, IntPtr.Zero, callbackPtr, IntPtr.Zero);
                return t;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void toplistbrowse_complete(IntPtr result, IntPtr userDataPtr)
        {
            try
            {
                libspotify.sp_error error = libspotify.sp_toplistbrowse_error(result);

                if (error != libspotify.sp_error.OK)
                {
                    return;
                }

                int count = this.ToplistType == libspotify.sp_toplisttype.SP_TOPLIST_TYPE_ALBUMS ? libspotify.sp_toplistbrowse_num_albums(_browsePtr) : this.ToplistType == libspotify.sp_toplisttype.SP_TOPLIST_TYPE_ARTISTS ? libspotify.sp_toplistbrowse_num_artists(_browsePtr) : libspotify.sp_toplistbrowse_num_tracks(_browsePtr);

                List<IntPtr> ptrs = new List<IntPtr>();

                IntPtr tmp = IntPtr.Zero;

                for (int i = 0; i < count; i++)
                {

                    if (this.ToplistType == libspotify.sp_toplisttype.SP_TOPLIST_TYPE_ALBUMS)
                    {
                        tmp = libspotify.sp_toplistbrowse_album(_browsePtr, i);
                        if (libspotify.sp_album_is_available(tmp))
                            ptrs.Add(tmp);
                    }
                    else if (this.ToplistType == libspotify.sp_toplisttype.SP_TOPLIST_TYPE_ARTISTS)
                    {
                        tmp = libspotify.sp_toplistbrowse_artist(_browsePtr, i);
                        ptrs.Add(tmp);

                    }
                    else
                    {
                        tmp = libspotify.sp_toplistbrowse_track(_browsePtr, i);
                        ptrs.Add(tmp);
                    }
                }

                this.Ptrs = ptrs;

                this.IsLoaded = true;
            }
            finally
            {
                safeReleaseToplist();
            }
        }

        private void safeReleaseToplist()
        {
            if (_browsePtr != IntPtr.Zero)
            {
                try
                {

                    libspotify.sp_toplistbrowse_release(_browsePtr);

                }
                catch
                { }
            }
        }
    }
}
