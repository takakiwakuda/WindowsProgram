<#
.SYNOPSIS
    Build script for WindowsProgram module.
#>
[CmdletBinding()]
param (
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]
    $Configuration = (property Configuration Release),

    [Parameter()]
    [ValidateSet("net462", "net7.0")]
    [string]
    $Framework
)

if ($Framework.Length -eq 0) {
    $Framework = if ($PSEdition -eq "Core") { "net7.0" } else { "net462" }
}

$PSCmdlet.WriteVerbose("Configuration : $Configuration")
$PSCmdlet.WriteVerbose("Framework     : $Framework")

<#
.SYNOPSIS
    Build WindowsProgram assembly.
#>
task BuildWindowsProgram @{
    Inputs  = {
        Get-ChildItem -Path WindowsProgram\*.cs, WindowsProgram\WindowsProgram.csproj
    }
    Outputs = "WindowsProgram\bin\$Configuration\$Framework\WindowsProgram.dll"
    Jobs    = {
        exec { dotnet publish -c $Configuration -f $Framework WindowsProgram }
    }
}

<#
.SYNOPSIS
    Build WindowsProgram module.
#>
task BuildModule BuildWindowsProgram, {
    $version = (Import-PowerShellDataFile -LiteralPath WindowsProgram\WindowsProgram.psd1).ModuleVersion
    $destination = "$PSScriptRoot\out\$Configuration\$Framework\WindowsProgram\$version"

    if (Test-Path -LiteralPath $destination -PathType Container) {
        Remove-Item -LiteralPath $destination -Recurse
    }
    $null = New-Item -Path $destination -ItemType Directory -Force

    $source = "WindowsProgram\bin\$Configuration\$Framework"

    Copy-Item -LiteralPath $source\WindowsProgram.dll -Destination $destination
    Copy-Item -LiteralPath $source\WindowsProgram.format.ps1xml -Destination $destination
    Copy-Item -LiteralPath $source\WindowsProgram.psd1 -Destination $destination

    if ($Framework -eq "net462") {
        Copy-Item -LiteralPath $source\System.Buffers.dll -Destination $destination
        Copy-Item -LiteralPath $source\System.Memory.dll -Destination $destination
        Copy-Item -LiteralPath $source\System.Numerics.Vectors.dll -Destination $destination
        Copy-Item -LiteralPath $source\System.Runtime.CompilerServices.Unsafe.dll -Destination $destination
    }
}

<#
.SYNOPSIS
    Run WindowsProgram module tests.
#>
task RunModuleTest BuildModule, {
    $command = @"
    & {
        Import-Module -Name '$PSScriptRoot\out\$Configuration\$Framework\WindowsProgram';
        Invoke-Pester -Path '$PSScriptRoot\test' -Output Detailed
    }
"@

    switch ($Framework) {
        "net7.0" {
            exec { pwsh -nop -c $command }
        }
        default {
            exec { powershell -noprofile -command $command }
        }
    }
}

<#
.SYNOPSIS
    Run default tasks.
#>
task . RunModuleTest
