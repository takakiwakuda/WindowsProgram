using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace WindowsProgram;

/// <summary>
/// The Get-WindowsProgram cmdlet gets installed programs on Windows.
/// </summary>
[Cmdlet(VerbsCommon.Get, "WindowsProgram", HelpUri = "https://github.com/takakiwakuda/WindowsProgram/blob/main/doc/Get-WindowsProgram.md")]
[OutputType(typeof(WindowsProgramInfo))]
public class GetWindowsProgramCommand : PSCmdlet
{
    /// <summary>
    /// <see cref="Cmdlet.ProcessRecord"/> override.
    /// Gets installed programs on Windows.
    /// </summary>
    protected override void ProcessRecord()
    {
        List<WindowsProgramInfo> infos = new();

        infos.AddRange(EnumeratePrograms(RegistryHive.CurrentUser, RegistryView.Default));
        infos.AddRange(EnumeratePrograms(RegistryHive.LocalMachine, RegistryView.Registry32));

        if (Environment.Is64BitOperatingSystem)
        {
            infos.AddRange(EnumeratePrograms(RegistryHive.LocalMachine, RegistryView.Registry64));
        }

        WriteObject(infos.OrderBy(i => i.Name), true);
    }

    private IEnumerable<WindowsProgramInfo> EnumeratePrograms(RegistryHive hKey, RegistryView view)
    {
        using RegistryKey baseKey = RegistryKey.OpenBaseKey(hKey, view);
        using RegistryKey? uninstallKey = baseKey.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall", false);

        if (uninstallKey is null)
        {
            // The following code may be executed.
            throw new Exception($"Unable to open key '{baseKey.Name}\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall'.");
        }

        foreach (string subKeyName in uninstallKey.GetSubKeyNames())
        {
            using RegistryKey? programKey = uninstallKey.OpenSubKey(subKeyName, false);

            if (programKey is null)
            {
                // The following code may be executed.
                WriteWarning($"Unable to open key named '{subKeyName}' in the '{uninstallKey.Name}'.");
                continue;
            }

            if ((int?)programKey.GetValue("SystemComponent", 0) == 1)
            {
                continue;
            }

            if (programKey.GetValue("ParentKeyName") is not null)
            {
                continue;
            }

            string name = (string?)programKey.GetValue("DisplayName") ?? string.Empty;
            if (name.Length == 0)
            {
                continue;
            }

            if (!Version.TryParse((string?)programKey.GetValue("DisplayVersion"), out Version? version))
            {
                version = null;
            }

            yield return new WindowsProgramInfo()
            {
                InstalledDate = GetInstalledDate(programKey),
                Name = name,
                Publisher = (string?)programKey.GetValue("Publisher"),
                Size = (int?)programKey.GetValue("EstimatedSize"),
                Version = version
            };
        }
    }

    [SuppressMessage("csharp", "IDE0057")]
    private DateTime GetInstalledDate(RegistryKey key)
    {
        string? datetime = (string?)key.GetValue("InstallDate");
        if (datetime is null || !Regex.IsMatch(datetime, "^[0-9]{8}$"))
        {
            return NativeMethod.GetLastWriteTime(key);
        }

        int year = int.Parse(datetime.Substring(0, 4));
        int month = int.Parse(datetime.Substring(4, 2));
        int day = int.Parse(datetime.Substring(6, 2));

        try
        {
            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
        }
        catch (ArgumentOutOfRangeException)
        {
            WriteDebug($"Datetime {datetime} is invalid.");
        }

        return NativeMethod.GetLastWriteTime(key);
    }
}

/// <summary>
/// Represents information about Windows program.
/// </summary>
public class WindowsProgramInfo
{
    /// <summary>
    /// Gets or sets the installed date of the program.
    /// </summary>
    public DateTime InstalledDate { get; init; }

    /// <summary>
    /// Gets or sets the name of the program.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets or sets the publisher of the program.
    /// </summary>
    public string? Publisher { get; init; }

    /// <summary>
    /// Gets or sets the size of the program.
    /// </summary>
    public int? Size { get; init; }

    /// <summary>
    /// Gets or sets the version of the program.
    /// </summary>
    public Version? Version { get; init; }
}
