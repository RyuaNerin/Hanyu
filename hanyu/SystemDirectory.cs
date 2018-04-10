using System;
using System.Runtime.InteropServices;

namespace hanyu
{
    internal static class SystemDirectory
    {
        private readonly static Guid localLowId = new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16");

        public static string LocalNow => GetKnownFolderPath(localLowId);

        private static string GetKnownFolderPath(Guid knownFolderId)
        {
            var pszPath = IntPtr.Zero;
            try
            {
                if (NativeMethods.SHGetKnownFolderPath(knownFolderId, 0, IntPtr.Zero, out pszPath) >= 0)
                    return Marshal.PtrToStringAuto(pszPath);

                return null;
            }
            finally
            {
                if (pszPath != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pszPath);
            }
        }

        private static class NativeMethods
        {
            [DllImport("shell32.dll")]
            public static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);
        }
    }
}
