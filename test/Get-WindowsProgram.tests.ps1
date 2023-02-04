Describe "Get-WindowsProgram tests" {
    It "Gets all installed programs in Windows" {
        $programs = Get-WindowsProgram
        $programs | Should -BeOfType "WindowsProgram.ProgramInfo"
    }

    It "Gets installed programs whose name starts with 'Microsoft'" {
        $programs = Get-WindowsProgram -Name "Microsoft*"
        $programs | Should -BeOfType "WindowsProgram.ProgramInfo"
        $programs.Name | Should -BeLike "Microsoft*"
    }
    It "Gets an installed program called 'Microsoft Edge'" {
        $programs = Get-WindowsProgram -Name "Microsoft Edge"
        $programs | Should -BeOfType "WindowsProgram.ProgramInfo"
        $programs.Count | Should -Be 1
    }
}
