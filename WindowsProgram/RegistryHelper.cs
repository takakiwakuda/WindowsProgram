using System;
using System.ComponentModel;
using Microsoft.Win32;

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
            out Kernetl32.FILETIME fileTime);

        if (error != 0)
        {
            throw new Win32Exception(error);
        }

        return fileTime.ToDateTime();
    }
}
