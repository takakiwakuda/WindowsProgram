using System;
using System.Runtime.InteropServices;

namespace WindowsProgram;

internal static class Kernetl32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct FILETIME
    {
        internal uint dwLowDateTime;
        internal uint dwHighDateTime;

        internal long ToTicks() => ((long)dwHighDateTime << 32) + dwLowDateTime;
        internal DateTime ToDateTime() => DateTime.FromFileTime(ToTicks());
    }
}
