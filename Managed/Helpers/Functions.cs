using MediaBrowser.Plugins.Spotify.Managed.Wrapper;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaBrowser.Plugins.Spotify.Manged
{
    internal static class Functions
    {
        internal static string PtrToString(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return String.Empty;

            List<byte> l = new List<byte>();
            byte read = 0;
            do
            {
                read = Marshal.ReadByte(ptr, l.Count);
                l.Add(read);
            }
            while (read != 0);

            if (l.Count > 0)
                return System.Text.Encoding.UTF8.GetString(l.ToArray(), 0, l.Count - 1);
            else
                return string.Empty;
        }

        internal static string LinkToString(IntPtr linkPtr)
        {
            byte[] buffer = new byte[128];
            IntPtr bufferPtr = IntPtr.Zero;

            try
            {

                bufferPtr = Marshal.AllocHGlobal(buffer.Length);

                int i = libspotify.sp_link_as_string(linkPtr, bufferPtr, buffer.Length);

                if (i == 0)
                    return null;

                Marshal.Copy(bufferPtr, buffer, 0, buffer.Length);

                return System.Text.Encoding.UTF8.GetString(buffer, 0, i);

            }
            finally
            {
                try
                {
                    if (bufferPtr != IntPtr.Zero)
                        Marshal.FreeHGlobal(bufferPtr);
                }
                catch { }
            }
        }

        internal static string GetCountryName(int country)
        {
            string countryCode = Encoding.ASCII.GetString(new byte[] { (byte)(country >> 8), (byte)(country & 0xff) });

            switch (countryCode)
            {

                case "US":

                    return "the United States";

                case "SE":

                    return "Sweden";

                case "FI":

                    return "Finland";

                case "ES":

                    return "Spain";

                case "FR":

                    return "France";

                case "NO":

                    return "Norway";

                case "GB":

                    return "the United Kingdom";

                case "NL":

                    return "the Netherlands";

                case "DK":

                    return "Denmark";

                case "AT":

                    return "Austria";

                case "BE":

                    return "Belgium";

                case "CH":

                    return "Switzerland";

                case "DE":

                    return "Germany";

                case "AU":

                    return "Australia";

                case "NZ":

                    return "New Zealand";

                default:

                    return "My Country";
            }
        }
    }
}
