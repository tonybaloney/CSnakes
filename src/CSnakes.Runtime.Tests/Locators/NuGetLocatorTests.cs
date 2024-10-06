using CSnakes.Runtime.Locators;
using Microsoft.TestUtilities;

namespace CSnakes.Runtime.Tests.Locators;

[Collection("Sequential")]
public sealed class NuGetLocatorTests : IDisposable
{
    private const string PythonNugetVersion = "3.9";
    private static readonly Version PythonVersion = new(3, 9);

    private const string UserProfileEnvVarName = "USERPROFILE";
    private const string NugetPackagesEnvVarName = "NUGET_PACKAGES";

    private readonly string? initialUserProfileEnvVar = Environment.GetEnvironmentVariable(UserProfileEnvVarName);
    private readonly string? initialNuGetPackagesEnvVar = Environment.GetEnvironmentVariable(NugetPackagesEnvVarName);

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(UserProfileEnvVarName, initialUserProfileEnvVar);
        Environment.SetEnvironmentVariable(NugetPackagesEnvVarName, initialNuGetPackagesEnvVar);
    }

    [ConditionalTheory]
    [OSSkipCondition(OperatingSystems.Linux | OperatingSystems.MacOSX)]
    [MemberData(nameof(GetValues))]
    public void LocatePython_returns_expected_when_environmentVariables_set(string envVarName, string envVarValue, string expectedPath)
    {
        if (!NugetPackagesEnvVarName.Equals(envVarValue, StringComparison.OrdinalIgnoreCase))
            Environment.SetEnvironmentVariable(NugetPackagesEnvVarName, null);

        Environment.SetEnvironmentVariable(envVarName, envVarValue);
        MockNuGetLocator locator = new(PythonNugetVersion, PythonVersion);

        PythonLocationMetadata result = locator.LocatePython();

        Assert.NotNull(result);
        Assert.Equal(expectedPath, result.Folder);
    }

    public static IEnumerable<object[]> GetValues()
    {
        yield return new object[] { UserProfileEnvVarName, @"C:\NuGetPackages", $@"C:\NuGetPackages\.nuget\packages\python\{PythonNugetVersion}\tools" };
        yield return new object[] { NugetPackagesEnvVarName, @"C:\NuGetPackages", $@"C:\NuGetPackages\python\{PythonNugetVersion}\tools" };
    }

    [ConditionalTheory]
    [OSSkipCondition(OperatingSystems.Linux | OperatingSystems.MacOSX)]
    [InlineData(null, null)]
    [InlineData(null, "")]
    [InlineData("", null)]
    [InlineData("", "")]
    public void LocatePython_should_throw_DirectoryNotFoundException_if_environmentVariables_unset(string? valueUSERPROFILE, string? valueNUGET_PACKAGES)
    {
        Environment.SetEnvironmentVariable(UserProfileEnvVarName, valueUSERPROFILE);
        Environment.SetEnvironmentVariable(NugetPackagesEnvVarName, valueNUGET_PACKAGES);
        MockNuGetLocator locator = new(PythonNugetVersion, PythonVersion);

        var ex = Assert.Throws<DirectoryNotFoundException>(locator.LocatePython);
        Assert.Equal($"Neither {NugetPackagesEnvVarName} or {UserProfileEnvVarName} environments variable were found, which are needed to locate the NuGet package cache.", ex.Message);
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
