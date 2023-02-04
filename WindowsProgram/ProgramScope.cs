namespace WindowsProgram;

/// <summary>
/// Specifies a scope for retrieving program infromation.
/// </summary>
public enum ProgramScope
{
    /// <summary>
    /// Not specified.
    /// </summary>
    None,

    /// <summary>
    /// Specifies the current user.
    /// </summary>
    CurrentUser,

    /// <summary>
    /// Specifies the local machine.
    /// </summary>
    Machine
}
