param(
    [int]$Port = 5046
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptRoot '..')
$projectPath = Join-Path $repoRoot 'src\CreadorDeRequerimientos.API\CreadorDeRequerimientos.API.csproj'

$ipv4Addresses = Get-NetIPAddress -AddressFamily IPv4 |
    Where-Object {
        $_.IPAddress -ne '127.0.0.1' -and
        $_.ValidLifetime -gt 0 -and
        $_.PrefixOrigin -ne 'WellKnown'
    } |
    Sort-Object InterfaceMetric, SkipAsSource |
    Select-Object -ExpandProperty IPAddress

$preferredIp = $ipv4Addresses | Select-Object -First 1

if (-not $preferredIp) {
    throw 'No encontre una IP local IPv4 util para compartir la app en la red.'
}

$env:ASPNETCORE_URLS = "http://0.0.0.0:$Port"
$env:ASPNETCORE_ENVIRONMENT = 'Development'

Write-Host ''
Write-Host 'Creador de Requerimientos listo para red local'
Write-Host "PC:      http://localhost:$Port"
Write-Host "Celular: http://${preferredIp}:$Port"
Write-Host ''
Write-Host 'Si Windows pregunta por el firewall, permite acceso en red privada.'
Write-Host 'Deja esta ventana abierta mientras uses la app desde el celular.'
Write-Host ''

Push-Location $repoRoot
try {
    dotnet run --project $projectPath --no-launch-profile
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
finally {
    Pop-Location
}
