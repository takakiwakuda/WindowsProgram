using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

namespace WindowsProgram;

internal static class NativeMethod
{
    private const int ERROR_SUCCESS = 0;

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
    private static extern int RegQueryInfoKey(
        SafeRegistryHandle hKey,
        StringBuilder? lpClass,
        uint lpcchClass,
        IntPtr lpReserved,
        IntPtr lpcSubKeys,
        IntPtr lpcbMaxSubKeyLen,
        IntPtr lpcbMaxClassLen,
        IntPtr lpcValues,
        IntPtr lpcbMaxValueNameLen,
        IntPtr lpcbMaxValueLen,
        IntPtr lpcbSecurityDescriptor,
        out System.Runtime.InteropServices.ComTypes.FILETIME lpftLastWriteTime
    );

    internal static DateTime GetLastWriteTime(RegistryKey key)
    {
        int error = RegQueryInfoKey(
            key.Handle,
            null,
            0,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            IntPtr.Zero,
            out System.Runtime.InteropServices.ComTypes.FILETIME ft
        );

        if (error != ERROR_SUCCESS)
        {
            throw new Win32Exception(error);
        }

        long time = ((long)ft.dwHighDateTime) << 32 | (uint)ft.dwLowDateTime;
        return DateTime.FromFileTime(time);
    }
}
