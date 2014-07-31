using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace MediaBrowser.Plugins.Spotify.Embedded
{
    public class EmbeddedAssembly
    {
        public static void Load(string embeddedResource, string fileName)
        {
            byte[] ba = null;          
            Assembly curAsm = Assembly.GetExecutingAssembly();

            using (Stream stm = curAsm.GetManifestResourceStream(embeddedResource))
            {                
                if (stm == null)
                    throw new Exception(embeddedResource + " is not found in Embedded Resources.");
                
                ba = new byte[(int)stm.Length];
                stm.Read(ba, 0, (int)stm.Length);               
            }

            bool fileOk = false;
            string tempFile = "";

            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                string fileHash = BitConverter.ToString(sha1.ComputeHash(ba)).Replace("-", string.Empty); ;

                tempFile = Path.GetTempPath() + fileName;

                if (File.Exists(tempFile))
                {
                    byte[] bb = File.ReadAllBytes(tempFile);
                    string fileHash2 = BitConverter.ToString(sha1.ComputeHash(bb)).Replace("-", string.Empty);

                    if (fileHash == fileHash2)
                    {
                        fileOk = true;
                    }
                    else
                    {
                        fileOk = false;
                    }
                }
                else
                {
                    fileOk = false;
                }
            }

            if (!fileOk)
            {
                System.IO.File.WriteAllBytes(tempFile, ba);
            }

            IntPtr h = LoadLibrary(tempFile);         
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string lpFileName);       
    }
}
