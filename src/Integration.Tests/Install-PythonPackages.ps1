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

$savedMSBuildTerminalLogger = $env:MSBUILDTERMINALLOGGER
$env:MSBUILDTERMINALLOGGER = 'off'

try {

$testProject = Join-Path $PSScriptRoot 'Integration.Tests.csproj'
$stageProject = Join-Path $PSScriptRoot '..' 'CSnakes.Stage' 'CSnakes.Stage.csproj'

# Determine the staging tool's target framework and build it (once)
$stageTfm = ((dotnet build $stageProject -getProperty:TargetFrameworks) -split ';')[0]
if (-not $stageTfm) {
    throw "No target frameworks found in $stageProject."
}

if (-not $NoBuild) {
    Write-Verbose "Building $testProject..."
    dotnet build $testProject
    Write-Verbose "Building $stageProject ($stageTfm)..."
    dotnet build $stageProject --framework $stageTfm
}

# Resolve the staging tool assembly path
$stageAssembly = dotnet build $stageProject -property:TargetFramework=$stageTfm -getProperty:TargetPath
if (-not $stageAssembly) {
    throw "Failed to resolve TargetPath for staging tool ($stageTfm)."
}

# Query target frameworks from the test project
$tfms = (dotnet build $testProject -getProperty:TargetFrameworks) -split ';' | Where-Object { $_ -ne '' }

if (-not $tfms) {
    throw "No target frameworks found in $testProject."
}

Write-Verbose "Target frameworks: $($tfms -join ', ')"

foreach ($tfm in $tfms) {
    # Query the output path for this TFM
    $outputPath = dotnet build $testProject -property:TargetFramework=$tfm -getProperty:OutputPath
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
        dotnet $stageAssembly `
            $(if ($VerbosePreference -ne 'SilentlyContinue') { '--verbose' }) `
            --python=$PythonVersion `
            --venv=$venvPath `
            --pip-requirements=requirements.txt
    }
    finally {
        Pop-Location
    }
}

}
finally {
    if ($null -eq $savedMSBuildTerminalLogger) {
        Remove-Item env:MSBUILDTERMINALLOGGER -ErrorAction Ignore
    }
    else {
        $env:MSBUILDTERMINALLOGGER = $savedMSBuildTerminalLogger
    }
}
