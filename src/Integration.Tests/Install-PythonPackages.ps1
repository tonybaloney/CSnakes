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

# Determine the staging tool's target framework and build it (once)
$stageTfm = ((dotnet msbuild $stageProject -getProperty:TargetFrameworks -nologo) -split ';')[0]
if (-not $stageTfm) {
    throw "No target frameworks found in $stageProject."
}

if (-not $NoBuild) {
    Write-Verbose "Building $testProject..."
    dotnet build $testProject
    Write-Verbose "Building $stageProject ($stageTfm)..."
    dotnet build $stageProject --framework $stageTfm
}

# Resolve the staging tool executable path
$stageOutputPath = dotnet msbuild $stageProject -property:TargetFramework=$stageTfm -getProperty:OutputPath -nologo
if (-not $stageOutputPath) {
    throw "Failed to resolve OutputPath for staging tool ($stageTfm)."
}
$stageExe = Join-Path $PSScriptRoot '..' 'CSnakes.Stage' $stageOutputPath 'CSnakes.Stage'

# Query target frameworks from the test project
$tfms = (dotnet msbuild $testProject -getProperty:TargetFrameworks -nologo) -split ';' | Where-Object { $_ -ne '' }

if (-not $tfms) {
    throw "No target frameworks found in $testProject."
}

Write-Verbose "Target frameworks: $($tfms -join ', ')"

foreach ($tfm in $tfms) {
    # Query the output path for this TFM
    $outputPath = dotnet msbuild $testProject -property:TargetFramework=$tfm -getProperty:OutputPath -nologo
    if (-not $outputPath) {
        throw "Failed to resolve OutputPath for target framework '$tfm'."
    }
    $pythonHome = Join-Path $PSScriptRoot $outputPath 'python'
    $venvName = ".venv-$PythonVersion"
    $venvPath = Join-Path $pythonHome $venvName

    Write-Verbose "[$tfm] Python home: $pythonHome"
    Write-Verbose "[$tfm] Venv path: $venvPath"

    Push-Location $pythonHome
    try {
        & $stageExe `
            --python=$PythonVersion `
            --venv=$venvPath `
            --pip-requirements=requirements.txt
    }
    finally {
        Pop-Location
    }
}
