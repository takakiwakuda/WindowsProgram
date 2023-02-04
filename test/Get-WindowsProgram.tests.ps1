Describe "Get-WindowsProgram tests" {
    It "Gets all installed programs in Windows" {
        $programs = Get-WindowsProgram
        $programs | Should -BeOfType "WindowsProgram.ProgramInfo"
    }
}
