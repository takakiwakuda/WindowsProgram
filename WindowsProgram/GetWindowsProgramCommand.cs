using System.Collections.Generic;
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
    /// Gets or sets the Name parameter. Specifies the name of programs.
    /// </summary>
    [Parameter(Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [ValidateNotNullOrEmpty]
    [SupportsWildcards]
    public string[]? Name
    {
        get => _programNames;
        set => _programNames = value;
    }

    private string[]? _programNames;
    private readonly ProgramInfo[] _programInfos;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetWindowsProgramCommand"/> class.
    /// </summary>
    public GetWindowsProgramCommand()
    {
        _programInfos = ProgramInfo.GetProgramInfos();
    }

    /// <summary>
    /// ProcessRecord override.
    /// </summary>
    protected override void ProcessRecord()
    {
        WriteObject(GetMatchingProgramInfos(), true);
    }

    private IEnumerable<ProgramInfo> GetMatchingProgramInfos()
    {
        if (_programNames is null)
        {
            return _programInfos;
        }

        HashSet<ProgramInfo> matchedProgramInfos = new();
        WildcardPattern wildcard;

        foreach (string pattern in _programNames)
        {
            wildcard = WildcardPattern.Get(pattern, WildcardOptions.IgnoreCase);

            foreach (var programInfo in _programInfos)
            {
                if (wildcard.IsMatch(programInfo.Name))
                {
                    matchedProgramInfos.Add(programInfo);
                }
            }
        }

        return matchedProgramInfos;
    }
}
