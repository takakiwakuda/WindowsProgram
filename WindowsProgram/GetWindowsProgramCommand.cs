using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Win32;

namespace WindowsProgram;

/// <summary>
/// The Get-WindowsProgram cmdlet retrieves installed programs in Windows.
/// </summary>
[Cmdlet(VerbsCommon.Get, "WindowsProgram")]
[Alias("Get-InstalledProgram")]
[OutputType(typeof(ProgramInfo))]
public sealed class GetWindowsProgramCommand : PSCmdlet
{
    private const string UninstallKeyName = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";

    /// <summary>
    /// Get or sets the Scope parameter.
    /// Specifies a scope for retrieving programs.
    /// </summary>
    [Parameter]
    public ProgramScope Scope { get; set; }

    /// <summary>
    /// ProcessRecord override.
    /// </summary>
    protected override void ProcessRecord()
    {
        switch (Scope)
        {
            case ProgramScope.CurrentUser:
                WriteObject(EnumerateInstalledPrograms(RegistryHive.CurrentUser, RegistryView.Default), true);
                break;

            case ProgramScope.Machine:
                WriteObject(EnumerateInstalledPrograms(RegistryHive.LocalMachine, RegistryView.Registry64), true);
                WriteObject(EnumerateInstalledPrograms(RegistryHive.LocalMachine, RegistryView.Registry32), true);
                break;

            default:
                WriteObject(EnumerateInstalledPrograms(RegistryHive.CurrentUser, RegistryView.Default), true);
                WriteObject(EnumerateInstalledPrograms(RegistryHive.LocalMachine, RegistryView.Registry64), true);
                WriteObject(EnumerateInstalledPrograms(RegistryHive.LocalMachine, RegistryView.Registry32), true);
                break;
        }
    }

    private IEnumerable<ProgramInfo> EnumerateInstalledPrograms(RegistryHive hKey, RegistryView view)
    {
        using var baseKey = RegistryKey.OpenBaseKey(hKey, view);
        using var uninstallKey = baseKey.OpenSubKey(UninstallKeyName, false)!;

        foreach (string subKeyName in uninstallKey.GetSubKeyNames())
        {
            var subKey = uninstallKey.OpenSubKey(subKeyName, false)!;

            if ((int)subKey.GetValue("SystemComponent", 0) == 1)
            {
                WriteDebug($"The program '{subKey}' is a system component.");
                subKey.Dispose();
                continue;
            }

            string? name = (string?)subKey.GetValue("DisplayName");
            if (name is null)
            {
                WriteDebug($"The program '{subKey}' does not have the 'DisplayName'.");
                subKey.Dispose();
                continue;
            }

            yield return new ProgramInfo(name, subKey);
        }
    }
}
