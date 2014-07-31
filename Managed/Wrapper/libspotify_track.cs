﻿/*-
 * Copyright (c) 2013 Software Development Solutions, Inc.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR OR CONTRIBUTORS BE LIABLE
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
 * SUCH DAMAGE.
 */

using System;
using System.Runtime.InteropServices;

namespace MediaBrowser.Plugins.Spotify.Managed.Wrapper
{
    public static partial class libspotify
    {
        [DllImport("libspotify")]
        public static extern bool sp_track_is_loaded(IntPtr trackPtr);
       
        [DllImport("libspotify")]
        public static extern sp_error sp_track_error(IntPtr trackPtr);

        [DllImport("libspotify")]
        public static extern sp_availability sp_track_get_availability(IntPtr sessionPtr, IntPtr trackPtr);

        [DllImport("libspotify")]
        public static extern bool sp_track_is_local(IntPtr sessionPtr, IntPtr trackPtr);

        [DllImport("libspotify")]
        public static extern bool sp_track_is_autolinked(IntPtr sessionPtr, IntPtr trackPtr);

        [DllImport("libspotify")]
        public static extern IntPtr sp_track_get_playable(IntPtr sessionPtr, IntPtr trackPtr);

        [DllImport("libspotify")]
        public static extern bool sp_track_is_starred(IntPtr sessionPtr, IntPtr trackPtr);

        [DllImport("libspotify")]
        public static extern sp_error sp_track_set_starred(IntPtr sessionPtr, IntPtr tracksArrayPtr, int num_tracks, bool star);

        [DllImport("libspotify")]
        public static extern int sp_track_num_artists(IntPtr trackPtr);

        [DllImport("libspotify")]
        public static extern IntPtr sp_track_artist(IntPtr trackPtr, int index);

        [DllImport("libspotify")]
        public static extern IntPtr sp_track_album(IntPtr trackPtr);

        [DllImport("libspotify")]
        public static extern IntPtr sp_track_name(IntPtr trackPtr);

        [DllImport("libspotify")]
        public static extern int sp_track_duration(IntPtr trackPtr);

        [DllImport("libspotify")]
        public static extern int sp_track_popularity(IntPtr trackPtr);

        [DllImport("libspotify")]
        public static extern int sp_track_disc(IntPtr trackPtr);

        [DllImport("libspotify")]
        public static extern int sp_track_index(IntPtr trackPtr);

        [DllImport("libspotify")]
        public static extern IntPtr sp_localtrack_create(string artist, string title, string album, int length);

        [DllImport("libspotify")]
        public static extern sp_error sp_track_add_ref(IntPtr trackPtr);

        [DllImport("libspotify")]
        public static extern sp_error sp_track_release(IntPtr trackPtr);            
    }
}
