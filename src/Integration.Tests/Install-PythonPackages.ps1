<#
.SYNOPSIS
    Pre-installs Python packages for integration tests.

.DESCRIPTION
    Discovers target frameworks and output paths from the Integration.Tests project,
    then runs CSnakes.Stage to create virtual environments and install pip packages
    for each target framework.

.PARAMETER PythonVersion
    The Python version to install (e.g., "3.12", "3.9").
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$PythonVersion,

    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

$testProject = Join-Path $PSScriptRoot 'Integration.Tests.csproj'
$stageProject = Join-Path $PSScriptRoot '..' 'CSnakes.Stage' 'CSnakes.Stage.csproj'

# Build the test project to ensure output directories and requirements.txt exist
if (-not $NoBuild) {
    Write-Host "Building $testProject..."
    dotnet build $testProject
}

# Query target frameworks from the test project
$tfms = (dotnet msbuild $testProject -getProperty:TargetFrameworks -nologo) -split ';'

Write-Host "Target frameworks: $($tfms -join ', ')"

foreach ($tfm in $tfms) {
    # Query the output path for this TFM
    $outputPath = dotnet msbuild $testProject -property:TargetFramework=$tfm -getProperty:OutputPath -nologo
    $pythonHome = Join-Path $PSScriptRoot $outputPath 'python'
    $venvName = ".venv-$PythonVersion"
    $venvPath = Join-Path $pythonHome $venvName

    Write-Host ''
    Write-Host "[$tfm] Python home: $pythonHome"
    Write-Host "[$tfm] Venv path: $venvPath"

    Push-Location $pythonHome
    try {
        dotnet run --project $stageProject --framework net8.0 -- `
            --python=$PythonVersion `
            --venv=$venvPath `
            --pip-requirements=requirements.txt
    }
    finally {
        Pop-Location
    }
}

Write-Host ''
Write-Host 'Python package pre-installation complete.'
