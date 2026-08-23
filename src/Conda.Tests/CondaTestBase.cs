using CSnakes.Runtime.Tests;

namespace Conda.Tests;

/// <seealso href="https://xunit.net/docs/shared-context.html#collection-fixture"/>
[CollectionDefinition(Name)]
public sealed class PythonEnvironmentCollection : ICollectionFixture<PythonEnvironmentFixture>
{
    public const string Name = "PythonEnvironment";
}

public class PythonEnvironmentFixture : PythonEnvironmentFixtureBase
{
    public PythonEnvironmentFixture() :
        base(home: Path.Join(Environment.CurrentDirectory, "python"),
             static context =>
             {
                 var condaEnv = Environment.GetEnvironmentVariable("CONDA") ?? string.Empty;
                 var pythonVersion = Environment.GetEnvironmentVariable("PYTHON_VERSION");
                 var isWindows = OperatingSystem.IsWindows();

                 if (string.IsNullOrEmpty(condaEnv))
                 {
                     var basePath = isWindows
                                  ? Environment.GetEnvironmentVariable("LOCALAPPDATA")
                                  : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                     if (basePath is { } someBasePath)
                     {
                         IEnumerable<string> condaNames = ["anaconda3", "miniconda3"];
                         condaEnv = condaNames.Select(condaName => Path.Join(someBasePath, condaName))
                                              .FirstOrDefault(Directory.Exists) ?? condaEnv;
                     }
                 }

                 var condaBinPath = isWindows ? Path.Join(condaEnv, "Scripts", "conda.exe") : Path.Join(condaEnv, "bin", "conda");
                 var environmentSpecPath = Path.Join(Environment.CurrentDirectory, "python", "environment.yml");

                 _ = context.Environment
                            .FromConda(condaBinPath)
                            .WithCondaEnvironment("csnakes_test", environmentSpecPath, true, pythonVersion);
             })
    {
    }
}

[Collection(PythonEnvironmentCollection.Name)]
public abstract class CondaTestBase(PythonEnvironmentFixture fixture)
{
    public IPythonEnvironment Env { get; } = fixture.Env;
}
