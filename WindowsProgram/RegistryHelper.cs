using System;
using System.ComponentModel;
using Microsoft.Win32;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace WindowsProgram;

internal static class RegistryHelper
{
    internal static DateTime GetLastWriteTime(RegistryKey key)
    {
        int error = Advapi32.RegQueryInfoKey(
            key.Handle,
            0,
            out _,
            0,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _,
            out _,
            out FILETIME ft);

        if (error != 0)
        {
            throw new Win32Exception(error);
        }

        long time = (((long)ft.dwHighDateTime) << 32) | (uint)ft.dwLowDateTime;
        return DateTime.FromFileTime(time);
    }
}
