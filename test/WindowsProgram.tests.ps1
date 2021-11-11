#Requires -Module WindowsProgram

#region Using directive
using namespace WindowsProgram
#endregion

Set-StrictMode -Version 3.0

Describe "WindowsProgram" {
    Context "Get-WindowsProgram" {
        It "Should retrieve installed programs on Windows" {
            Get-WindowsProgram | Should -BeOfType ([WindowsProgramInfo])
        }
    }
}
