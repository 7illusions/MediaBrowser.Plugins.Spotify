using MediaBrowser.Controller.Plugins;
using MediaBrowser.Plugins.Spotify.Embedded;
using MediaBrowser.Plugins.Spotify.Managed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.Spotify
{
    public class ServerEntryPoint : IServerEntryPoint
    {
        public void Run()
        {
            Manager.Initialize();
            if (Plugin.Instance.Configuration.Enabled && Plugin.Instance.Configuration.ValidSetup)
            {
                var key = EmbeddedResource.Load("MediaBrowser.Plugins.Spotify.Embedded.spotify_appkey.key");
                Manager.Login(key, Plugin.Instance.Configuration.UserName, Plugin.Instance.Configuration.Password);
            }            
        }

        public void Dispose()
        {
            Manager.ShutDown();            
        }
    }
}
