using MediaBrowser.Model.Plugins;
using System;
using System.Text;
using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Spotify.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {

        public string UserName { get; set; }


        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [XmlIgnore]
        public string Password 
        { 
            get
            {
                var bytes = Convert.FromBase64String(EncPassword);
                return Encoding.UTF8.GetString(bytes);          
            }
            set
            {
                if (value == null)
                    return;
                var bytes = UTF8Encoding.UTF8.GetBytes(value);
                EncPassword = Convert.ToBase64String(bytes);
            }
        }

        public string EncPassword { get; set; }

        public bool Enabled
        { get; set; }

        public bool ValidSetup
        { get; set; }

        public PluginConfiguration()
        {
            Enabled = false;
            ValidSetup = false;
            UserName = "";
            EncPassword = "";            
        }
    }
}
