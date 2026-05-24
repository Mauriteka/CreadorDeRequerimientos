$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Resolve-VabelRoutesMSBuild {
    if (-not [string]::IsNullOrWhiteSpace($env:VABEL_MSBUILD_PATH) -and (Test-Path $env:VABEL_MSBUILD_PATH)) {
        return (Resolve-Path $env:VABEL_MSBUILD_PATH).Path
    }

    $candidates = @()

    $vsWhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path $vsWhere) {
        $installations = & $vsWhere -products * -prerelease -format json | ConvertFrom-Json
        foreach ($installation in $installations) {
            if ($installation.installationPath) {
                $candidates += Join-Path $installation.installationPath 'MSBuild\Current\Bin\amd64\MSBuild.exe'
                $candidates += Join-Path $installation.installationPath 'MSBuild\Current\Bin\MSBuild.exe'
            }
        }
    }

    $candidates += @(
        'G:\Visual Studio 2026\Community\MSBuild\Current\Bin\amd64\MSBuild.exe',
        'G:\Visual Studio 2026\Community\MSBuild\Current\Bin\MSBuild.exe',
        'C:\Program Files\Microsoft Visual Studio\2026\Community\MSBuild\Current\Bin\amd64\MSBuild.exe',
        'C:\Program Files\Microsoft Visual Studio\2026\Community\MSBuild\Current\Bin\MSBuild.exe',
        'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\amd64\MSBuild.exe',
        'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe'
    )

    foreach ($candidate in $candidates | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique) {
        if (Test-Path $candidate) {
            return (Resolve-Path $candidate).Path
        }
    }

    return $null
}

$resolvedMSBuild = Resolve-VabelRoutesMSBuild
if ($resolvedMSBuild) {
    $env:VABEL_MSBUILD_PATH = $resolvedMSBuild
    Write-Host "Using MSBuild: $resolvedMSBuild"
}
else {
    Write-Host "Visual Studio MSBuild was not found. Falling back to dotnet CLI."
}
