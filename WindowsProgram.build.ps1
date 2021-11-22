[CmdletBinding()]
param (
    [Parameter()]
    [ValidateSet("Debug", "Release")]
    [string]
    $Configuration = "Debug",

    [Parameter()]
    [ValidateSet("net6.0", "net462")]
    [string]
    $Framework = "net6.0"
)

task Build @{
    Inputs  = Get-ChildItem -Path *.cs, *.csproj
    Outputs = "bin\$Configuration\$Framework\WindowsProgram.dll"
    Jobs    = {
        exec { dotnet publish --nologo -c $Configuration -f $Framework }
    }
}

task Clean {
    remove bin, obj, out
}

task Pack {
    $manifest = "bin\$Configuration\$Framework\WindowsProgram.psd1"
    $version = (Import-PowerShellDataFile -LiteralPath $manifest).ModuleVersion
    $output = "out\$Configuration\$Framework\WindowsProgram\$version"

    if (Test-Path -LiteralPath $output -PathType Container) {
        Remove-Item -Path $output\* -Recurse -Force
    }
    else {
        New-Item -Path $output -ItemType Directory > $null
    }

    $params = @{
        Path        = "bin\$Configuration\$Framework\publish\WindowsProgram.*", "en-US", "ja-JP"
        Destination = $output
        Recurse     = $true
    }
    Copy-Item @params
}

task Test {
    $module = "$PSScriptRoot\out\$Configuration\$Framework\WindowsProgram"
    $command = "& { Import-Module -Name '$module'; Invoke-Pester -Path '$PSScriptRoot' }"

    switch ($Framework) {
        "net6.0" {
            exec { pwsh -NoProfile -c $command }
        }

        "net462" {
            exec { powershell -NoProfile -Command $command }
        }
    }
}

task . Build, Pack
