using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace WindowsProgram;

internal static partial class Advapi32
{
#if NET7_0_OR_GREATER
    [LibraryImport(nameof(Advapi32), EntryPoint = "RegQueryInfoKeyW", SetLastError = true)]
    internal static partial int RegQueryInfoKey(
        SafeRegistryHandle hKey,
        nint lpClass,
        out uint lpcchClass,
        nint lpReserved,
        out uint lpcSubKeys,
        out uint lpcbMaxSubKeyLen,
        out uint lpcbMaxClassLen,
        out uint lpcValues,
        out uint lpcbMaxValueNameLen,
        out uint lpcbMaxValueLen,
        out nint lpcbSecurityDescriptor,
        out Kernetl32.FILETIME lpftLastWriteTime);
#else
    [DllImport(nameof(Advapi32), EntryPoint = "RegQueryInfoKeyW", SetLastError = true)]
    internal static extern int RegQueryInfoKey(
        SafeRegistryHandle hKey,
        nint lpClass,
        out uint lpcchClass,
        nint lpReserved,
        out uint lpcSubKeys,
        out uint lpcbMaxSubKeyLen,
        out uint lpcbMaxClassLen,
        out uint lpcValues,
        out uint lpcbMaxValueNameLen,
        out uint lpcbMaxValueLen,
        out nint lpcbSecurityDescriptor,
        out Kernetl32.FILETIME lpftLastWriteTime);
#endif
}
