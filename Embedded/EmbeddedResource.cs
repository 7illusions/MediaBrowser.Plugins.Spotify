using System;
using System.IO;
using System.Reflection;

namespace MediaBrowser.Plugins.Spotify.Embedded
{
    class EmbeddedResource
    {
        public static byte[] Load(string embeddedResource)
        {            
            byte[] bytes = null;
            
            Assembly curAsm = Assembly.GetExecutingAssembly();

            using (Stream stm = curAsm.GetManifestResourceStream(embeddedResource))
            {
                if (stm == null)
                    throw new Exception(embeddedResource + " is not found in Embedded Resources.");

                bytes = new byte[(int)stm.Length];
                stm.Read(bytes, 0, (int)stm.Length);                             
            }

            return bytes;
        }
    }
}
