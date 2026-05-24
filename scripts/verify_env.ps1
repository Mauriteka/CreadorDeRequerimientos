param(
    [switch]$SkipRestore
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptRoot '..')
$solution = Join-Path $repoRoot 'CreadorDeRequerimientos.slnx'
$dotnetArgs = @('-m:1', '/nr:false', '/p:UseSharedCompilation=false')

Push-Location $repoRoot
try {
    Write-Host "Repository root: $repoRoot"

    if (-not $SkipRestore) {
        Write-Host 'Running dotnet restore...'
        dotnet restore $solution
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }

    Write-Host 'Building solution...'
    dotnet build $solution @dotnetArgs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
finally {
    Pop-Location
}
