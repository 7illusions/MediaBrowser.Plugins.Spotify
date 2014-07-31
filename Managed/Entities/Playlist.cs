using MediaBrowser.Plugins.Spotify.Managed.Entities;
using MediaBrowser.Plugins.Spotify.Managed.Wrapper;
using MediaBrowser.Plugins.Spotify.Manged;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MediaBrowser.Plugins.Spotify.Managed.Entities
{

    public class Playlist : IDisposable 
    {             
        private delegate void playlist_renamed_delegate(IntPtr playlistPtr, IntPtr userDataPtr);
       
        private playlist_renamed_delegate fn_playlist_renamed;        
                              
        private IntPtr _callbacksPtr;
        private bool _disposed;

        public string Name { get; private set; }
        public int TrackCount { get; private set; }
        public string Description { get; private set; }
        public int SubscriberCount { get; private set; }
        public bool IsInRAM { get; private set; }
        public libspotify.sp_playlist_offline_status OfflineStatus { get; private set; }       
        public IntPtr Pointer { get; private set; }

        private List<Track> _tracks;

        #region IDisposable Members

        public void Dispose() {

            dispose(true);
            GC.SuppressFinalize(this);

        }

        ~Playlist() {

            dispose(false);

        }

        private void dispose(bool disposing) {

            if(!_disposed) {

                if(disposing) {

                    SafeRemoveCallbacks();
                    
                }

                _disposed = true;

            }
                        
        }

        #endregion

        public static Playlist Get(IntPtr playlistPtr) {

            Playlist p = new Playlist((IntPtr)playlistPtr);           
            return p;

        }
               
        private Playlist(IntPtr playlistPtr) {

            this.Pointer = playlistPtr;

            InitCallbacks();

            if (this.IsLoaded) {

                PopulateMetadata();
                return;

            }
            
        }

        public bool IsLoaded 
        {
            get 
            { 
                return this.Pointer != IntPtr.Zero && libspotify.sp_playlist_is_loaded(this.Pointer);
            }
        }

        public bool TracksAreLoaded 
        {
            get 
            {
                if (this.Pointer == IntPtr.Zero || !this.IsLoaded)
                    return false;

                for (int i = 0; i < this.TrackCount; i++)
                {
                    IntPtr trackPtr = libspotify.sp_playlist_track(this.Pointer, i);
                    if (!libspotify.sp_track_is_loaded(trackPtr))
                        return false;
                }
                return true;
            }
        }

        private void InitCallbacks() 
        {

            if (this.Pointer == IntPtr.Zero)
                throw new InvalidOperationException("Invalid playlist pointer.");            


            this.fn_playlist_renamed = new playlist_renamed_delegate(Playlist_Renamed);
                       

            libspotify.sp_playlist_callbacks callbacks = new libspotify.sp_playlist_callbacks();

          
            callbacks.playlist_renamed = Marshal.GetFunctionPointerForDelegate(fn_playlist_renamed);  
         
            _callbacksPtr = Marshal.AllocHGlobal(Marshal.SizeOf(callbacks));
            Marshal.StructureToPtr(callbacks, _callbacksPtr, true);

            libspotify.sp_playlist_add_callbacks(this.Pointer, _callbacksPtr, IntPtr.Zero);

        }

        private void SafeRemoveCallbacks() 
        {
            try 
            {
                if (this.Pointer == IntPtr.Zero)
                    return;

                if (_callbacksPtr == IntPtr.Zero)
                    return;

                libspotify.sp_playlist_remove_callbacks(this.Pointer, _callbacksPtr, IntPtr.Zero);

            } catch { }

        }

        public List<Track> GetTracks() 
        {
            if (_tracks == null) 
            {

                _tracks = new List<Track>();

                for (int i = 0; i < this.TrackCount; i++) 
                {

                    IntPtr trackPtr = libspotify.sp_playlist_track(this.Pointer, i);

                    _tracks.Add(new Track(trackPtr));
                }
            }
            return _tracks;            
        }

        private void PopulateMetadata() 
        {
            this.Name = Functions.PtrToString(libspotify.sp_playlist_name(this.Pointer));
            this.TrackCount = libspotify.sp_playlist_num_tracks(this.Pointer);
            this.Description = Functions.PtrToString(libspotify.sp_playlist_get_description(this.Pointer));
            this.SubscriberCount = (int)libspotify.sp_playlist_num_subscribers(this.Pointer);
            this.IsInRAM = libspotify.sp_playlist_is_in_ram(Session.GetSessionPtr(), this.Pointer);
            this.OfflineStatus = libspotify.sp_playlist_get_offline_status(Session.GetSessionPtr(), this.Pointer);
            this.TrackCount = libspotify.sp_playlist_num_tracks(this.Pointer);
        }

        private void Playlist_Renamed(IntPtr playlistPtr, IntPtr userDataPtr) 
        {
            PopulateMetadata();
        }                           
    }

}
