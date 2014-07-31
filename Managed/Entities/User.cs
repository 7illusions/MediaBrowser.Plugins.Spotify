using MediaBrowser.Plugins.Spotify.Managed.Wrapper;
using MediaBrowser.Plugins.Spotify.Manged;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediaBrowser.Plugins.Spotify.Managed.Entities
{
    public class User 
    {

        public string CanonicalName { get; private set; }
        public string DisplayName { get; private set; }        
        public IntPtr UserPtr { get; private set; }

        public User(IntPtr userPtr) {

            if(!libspotify.sp_user_is_loaded(userPtr))
                throw new InvalidOperationException("User is not loaded.");

            this.UserPtr = userPtr;
            this.CanonicalName = Functions.PtrToString(libspotify.sp_user_canonical_name(userPtr));
            this.DisplayName = Functions.PtrToString(libspotify.sp_user_display_name(userPtr));            

        }

    }

}
