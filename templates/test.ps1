#Requires -PSEdition Core
#Requires -Version 7.0

<#
.SYNOPSIS
Tests and verifies the templates package by installing, testing template
scenarios, and building generated projects.

.DESCRIPTION
This script automates the testing process for the CSnakes.Templates package. It
builds and packs the template package, installs it temporarily, runs various
template scenarios to verify they work correctly, builds the generated projects,
and then cleans up by uninstalling the package.

The script runs multiple test cases with different template arguments to ensure
the templates work as expected with various options.

.PARAMETER NoDiffTool
Disables the diff tool during template verification. This is automatically
enabled when running in CI environments.

.PARAMETER OutputDirectory
Specifies the directory where test outputs will be created. Default is 'tmp'.
The directory must not already exist or the script will fail.

.PARAMETER KeepOutput
Prevents the script from deleting the output directory after completion.Useful
for debugging or inspecting generated template outputs.

.PARAMETER NoRestore
Skips restoring .NET tools before running the script. Use this if tools are
already restored.

.PARAMETER NoBuild
Skips building the generated projects after template verification.
Useful for faster execution when only template generation needs to be tested.

.INPUTS
None. This script does not accept pipeline input.

.OUTPUTS
Console output showing the progress of template testing and verification.
#>

[CmdletBinding()]
param (
    [switch]$NoDiffTool = $false,
    [string]$OutputDirectory = 'tmp',
    [switch]$KeepOutput = $false,
    [switch]$NoRestore = $false,
    [switch]$NoBuild = $false
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

try
{
    Push-Location $PSScriptRoot

    if ($noRestore) {
        Write-Verbose "Restoring tools."
        dotnet tool restore
    }

    if (Test-Path env:CI) {
        $noDiffTool = $true
    }

    Write-Verbose "Building and packing."
    $packArgs = @()
    if ($noRestore) {
        $packArgs += '--no-restore'
    }
    dotnet pack @packArgs

    $properties = (
        dotnet msbuild src\CSnakes.Templates.csproj -getProperty:PackageId,PackageVersion,PackageOutputPath
        | ConvertFrom-Json
        | Select-Object -ExpandProperty Properties
    )

    $packageId = $properties.PackageId
    $packageVersion = $properties.PackageVersion
    $packageDirPath = Resolve-Path -RelativeBasePath src $properties.PackageOutputPath
    $packageFilePath = Join-Path $packageDirPath "$packageId.$packageVersion.nupkg"

    Write-Verbose "Installing: $packageFilePath"
    dotnet new install $packageFilePath

    dotnet new pyapp -h

    $shouldRemoveOutput = $false

    try
    {
        if (Test-Path $outputDirectory) {
            throw "The output directory `"$outputDirectory`" already exists. Please remove it or specify a different one."
        }

        $testCases = @(
            @{ Name = 'pyapp'; Arguments = '' }
            @{ Name = 'pyapp'; Arguments = '--PythonVersion 3.10' }
            @{ Name = 'pyapp'; Arguments = '--NoVirtualEnvironment' }
            @{ Name = 'pyapp'; Arguments = '--PackageManager pip' }
            @{ Name = 'pyapp'; Arguments = '--PackageManager uv' }
            @{ Name = 'pyapp'; Arguments = '--NoVirtualEnvironment --PackageManager pip' }
        )

        $invalidFileNamePattern = "[$([Text.RegularExpressions.Regex]::Escape([IO.Path]::GetInvalidFileNameChars() -join ''))]"

        $errorCount = 0

        & {
            $ErrorActionPreference = 'Continue'

            $testCases |
                % { [pscustomobject]$_ } |
                Add-Member -PassThru ScriptProperty Scenario {
                    # https://github.com/dotnet/templating/blob/f97402028e679cb9cfb59388c27d08222aa46a39/tools/Microsoft.TemplateEngine.Authoring.TemplateVerifier/VerificationEngine.cs#L273-L282
                    ($this.Arguments -replace $invalidFileNamePattern, '') -replace ' ', '#'
                } |
                % {
                    Write-Output "$($_.Name): $($_.Arguments)"

                    $options = @()

                    if ($noDiffTool) {
                        $options += '--disable-diff-tool'
                    }

                    if ($_.Arguments.Length -gt 0) {
                        $options += '--template-args'
                        $options += $_.Arguments
                    }

                    $scenarioOutputDirPath = Join-Path $outputDirectory "$($_.Name).$($_.Scenario)"

                    $script:shouldRemoveOutput = -not $keepOutput

                    dotnet tool run dotnet-template-authoring verify `
                        $_.Name -d tests -o $scenarioOutputDirPath @options

                    if ($LASTEXITCODE -ne 0)
                    {
                        Write-Error "Template verification failed for scenario `"$($_.Name)`" with arguments: $($_.Arguments)"
                        $script:errorCount++
                    }
                }
        }

        if ($errorCount -gt 0) {
            throw "Template verification failed for $errorCount/$($testCases.Count) scenarios."
        }

        if (!$noBuild)
        {
            Get-ChildItem -Recurse -File -Filter *.csproj $outputDirectory |
            % {
                Write-Verbose "Building project: $($_.FullName)"
                dotnet build $_
            }
        }
    }
    finally
    {
        Write-Verbose "Uninstalling: $packageId"
        dotnet new uninstall $packageId
        if ($shouldRemoveOutput) {
            Remove-Item -ErrorAction Continue -Path $outputDirectory -Recurse -Force
        }
    }
}
finally
{
    Pop-Location
}
