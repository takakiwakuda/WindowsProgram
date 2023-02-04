Describe "Get-WindowsProgram tests" {
    It "Gets all installed programs in Windows" {
        $programs = Get-WindowsProgram
        $programs | Should -BeOfType "WindowsProgram.ProgramInfo"
    }

    It "Gets installed programs in the current user's scope" {
        $programs = Get-WindowsProgram -Scope CurrentUser
        $programs | Should -BeOfType "WindowsProgram.ProgramInfo"
        $programs.RegistryKey.Name | Should -BeLike "HKEY_CURRENT_USER\*"
    }

    It "Gets installed programs in the machine's scope" {
        $programs = Get-WindowsProgram -Scope Machine
        $programs | Should -BeOfType "WindowsProgram.ProgramInfo"
        $programs.RegistryKey.Name | Should -BeLike "HKEY_LOCAL_MACHINE\*"
    }
}
