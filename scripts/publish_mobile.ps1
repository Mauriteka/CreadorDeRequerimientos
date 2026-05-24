param(
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptRoot '..')
$projectPath = Join-Path $repoRoot 'src\CreadorDeRequerimientos.API\CreadorDeRequerimientos.API.csproj'
$outputPath = Join-Path $repoRoot 'artifacts\publish\mobile'

Push-Location $repoRoot
try {
    Write-Host "Publicando API en: $outputPath"
    dotnet publish $projectPath -c $Configuration -o $outputPath
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    Write-Host ''
    Write-Host 'Publicacion terminada.'
    Write-Host "Salida: $outputPath"
    Write-Host 'Para correrla en otra maquina Windows con .NET instalado:'
    Write-Host '  $env:ASPNETCORE_URLS="http://0.0.0.0:5046"'
    Write-Host '  .\CreadorDeRequerimientos.API.exe'
}
finally {
    Pop-Location
}
