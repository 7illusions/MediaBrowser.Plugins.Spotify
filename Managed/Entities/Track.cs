
using MediaBrowser.Plugins.Spotify.Managed.Wrapper;
using MediaBrowser.Plugins.Spotify.Manged;
using System;
using System.Collections.Generic;

namespace MediaBrowser.Plugins.Spotify.Managed.Entities
{
    public class Track
    {
        private List<string> _artists = new List<string>();

        public IntPtr TrackPtr { get; private set; }
        public string Name { get; private set; }
        public Album Album { get; private set; }
        public int TrackNumber { get; private set; }
        public decimal Seconds { get; private set; }

        public string[] Artists
        {

            get { return _artists.ToArray(); }

        }

        public Track(IntPtr trackPtr)
        {

            if (!libspotify.sp_track_is_loaded(trackPtr))
                throw new InvalidOperationException("Track is not loaded.");

            this.TrackPtr = trackPtr;
            this.Name = Functions.PtrToString(libspotify.sp_track_name(trackPtr));
            this.TrackNumber = libspotify.sp_track_index(trackPtr);
            this.Seconds = (decimal)libspotify.sp_track_duration(trackPtr) / 1000M;
            IntPtr albumPtr = libspotify.sp_track_album(trackPtr);
            if (albumPtr != IntPtr.Zero)
                this.Album = new Album(albumPtr);

            for (int i = 0; i < libspotify.sp_track_num_artists(trackPtr); i++)
            {

                IntPtr artistPtr = libspotify.sp_track_artist(trackPtr, i);
                if (artistPtr != IntPtr.Zero)
                    _artists.Add(Functions.PtrToString(libspotify.sp_artist_name(artistPtr)));

            }

        }
    }

}
