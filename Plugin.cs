using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Plugins.Spotify.Configuration;

namespace MediaBrowser.Plugins.Spotify
{
    public class Plugin : BasePlugin<PluginConfiguration>
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static Plugin Instance { get; set; } 

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public override string Name
        {
            get { return "Spotify"; }
        }
    }
}
