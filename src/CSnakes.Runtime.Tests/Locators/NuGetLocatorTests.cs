using CSnakes.Runtime.Locators;
using Microsoft.TestUtilities;

namespace CSnakes.Runtime.Tests.Locators;

[Collection("Sequential")]
public class NuGetLocatorTests
{
    private const string PythonNugetVersion = "3.9";
    private static readonly Version PythonVersion = new(3, 9);

    [ConditionalTheory]
    [OSSkipCondition(OperatingSystems.Linux | OperatingSystems.MacOSX)]
    [MemberData(nameof(GetValues))]
    public void LocatePython_returns_expected_when_environmentVariables_set(string envVarName, string envVarValue, string expectedPath)
    {
        string? presetValue = Environment.GetEnvironmentVariable(envVarName);

        try
        {
            Environment.SetEnvironmentVariable(envVarName, envVarValue);
            MockNuGetLocator locator = new(PythonNugetVersion, PythonVersion);

            PythonLocationMetadata result = locator.LocatePython();

            Assert.NotNull(result);
            Assert.Equal(expectedPath, result.Folder);
        }
        finally
        {
            Environment.SetEnvironmentVariable(envVarName, presetValue);
        }
    }

    public static IEnumerable<object[]> GetValues()
    {
        yield return new object[] { "USERPROFILE", @"C:\NuGetPackages", $@"C:\NuGetPackages\.nuget\packages\python\{PythonNugetVersion}\tools" };
        yield return new object[] { "NUGET_PACKAGES", @"C:\NuGetPackages", $@"C:\NuGetPackages\python\{PythonNugetVersion}\tools" };
    }

    [ConditionalTheory]
    [OSSkipCondition(OperatingSystems.Linux | OperatingSystems.MacOSX)]
    [InlineData(null, null)]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "")]
    public void LocatePython_should_throw_DirectoryNotFoundException_if_environmentVariables_unset(string? valueUSERPROFILE, string? valueNUGET_PACKAGES)
    {
        string? presetUSERPROFILE = Environment.GetEnvironmentVariable("USERPROFILE");
        string? presetNUGET_PACKAGES = Environment.GetEnvironmentVariable("NUGET_PACKAGES");

        try
        {
            Environment.SetEnvironmentVariable("USERPROFILE", valueUSERPROFILE);
            Environment.SetEnvironmentVariable("NUGET_PACKAGES", valueNUGET_PACKAGES);
            MockNuGetLocator locator = new(PythonNugetVersion, PythonVersion);

            var ex = Assert.Throws<DirectoryNotFoundException>(locator.LocatePython);
            Assert.Equal("Neither NUGET_PACKAGES or USERPROFILE environments variable were found, which are needed to locate the NuGet package cache.", ex.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable("USERPROFILE", presetUSERPROFILE);
            Environment.SetEnvironmentVariable("NUGET_PACKAGES", presetNUGET_PACKAGES);
        }
    }

    [ConditionalFact]
    [OSSkipCondition(OperatingSystems.Linux | OperatingSystems.MacOSX)]
    public void IsSupported_Windows_should_return_expected()
    {
        NuGetLocator locator = new(PythonNugetVersion, PythonVersion);

        Assert.True(locator.IsSupported());
    }

    [ConditionalFact]
    [OSSkipCondition(OperatingSystems.Windows)]
    public void IsSupported_not_Windows_should_return_expected()
    {
        NuGetLocator locator = new(PythonNugetVersion, PythonVersion);

        Assert.False(locator.IsSupported());
    }

    private class MockNuGetLocator : NuGetLocator
    {
        public MockNuGetLocator(string nugetVersion, Version version)
            : base(nugetVersion, version)
        {
        }

        protected override PythonLocationMetadata LocatePythonInternal(string folder, bool freeThreaded = false)
            => new(folder, PythonVersion, "", "", "");
    }
}
