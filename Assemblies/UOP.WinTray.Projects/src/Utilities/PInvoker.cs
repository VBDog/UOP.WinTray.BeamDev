using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UOP.WinTray.Projects.Utilities
{
    /// <summary>
    /// Maintains all Win32 API's required for the Md project
    /// </summary>
    public static class PInvoker
    {
        /// <summary>
        /// used to extract an string value from the passed INI formatted file
        /// does not raise an error if the file or key is not found just returns the default value.
        /// </summary>
        /// <param name="sectionName">Section in the file to extract a value from</param>
        /// <param name="keyName">Name of the value to extract a value from</param>
        /// <param name="defaultValue">Optional default value to return if the key is not found</param>
        /// <param name="returnedValue">returns false if the passed key is not found</param>
        /// <param name="size"></param>
        /// <param name="filePath">Path to an INI formatted text file</param>
        /// <returns></returns>

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string sectionName,
                                                            string keyName,
                                                            string defaultValue,
                                                            StringBuilder returnedValue,
                                                            int size,
                                                            string filePath);
        [DllImport("kernel32")]
        internal static extern long WritePrivateProfileString(string section, string key, StringBuilder val, string filePath);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);
    }
}
