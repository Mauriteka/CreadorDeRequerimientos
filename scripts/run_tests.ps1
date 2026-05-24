param(
    [switch]$SkipRestore
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptRoot '..')
$dotnetArgs = @('-m:1', '/nr:false', '/p:UseSharedCompilation=false')
$testProjects = @(
    'tests/Users.UnitTests/VABELRoutes.Application.Users.UnitTests.csproj',
    'tests/VABELRoutes.Application.Customers.UnitTests/VABELRoutes.Application.Customers.UnitTests.csproj',
    'tests/VABELRoutes.Application.ImageCompression.UnitTests/VABELRoutes.Application.ImageCompression.UnitTests.csproj',
    'tests/VABELRoutes.Application.Products.UnitTests/VABELRoutes.Application.Products.UnitTests.csproj',
    'tests/VABELRoutes.Application.UserTypes.UnitTests/VABELRoutes.Application.UserTypes.UnitTests.csproj'
)

Push-Location $repoRoot
try {
    Write-Host "Repository root: $repoRoot"
    foreach ($project in $testProjects) {
        Write-Host "Running tests for: $project"
        $testArgs = @($project)
        if ($SkipRestore) {
            $testArgs += '--no-restore'
        }
        $testArgs += $dotnetArgs
        dotnet test @testArgs
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    }
}
finally {
    Pop-Location
}
