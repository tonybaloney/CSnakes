namespace CSnakes.Tests;

public class TestEnvironment : IDisposable
{
    private readonly DirectoryInfo tempDir;

    public TestEnvironment()
    {
        tempDir = Directory.CreateTempSubdirectory("CSnakes");
        tempDir.Create();
    }

    public void Dispose()
    {
        Directory.Delete(tempDir.FullName, true);
    }

    public string TempDir => tempDir.FullName;
}
