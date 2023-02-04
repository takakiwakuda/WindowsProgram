using System.Management.Automation;

namespace WindowsProgram;

/// <summary>
/// The Get-WindowsProgram cmdlet retrieves installed programs in Windows.
/// </summary>
[Cmdlet(VerbsCommon.Get, "WindowsProgram")]
[Alias("Get-InstalledProgram")]
[OutputType(typeof(ProgramInfo))]
public sealed class GetWindowsProgramCommand : PSCmdlet
{
    /// <summary>
    /// ProcessRecord override.
    /// </summary>
    protected override void ProcessRecord()
    {
        WriteObject(ProgramInfo.GetProgramInfos(), true);
    }
}
