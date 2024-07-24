namespace PythonSourceGenerator.Tests
{
    public class TestEnvironment : IDisposable
    {
        private DirectoryInfo tempDir;

        public TestEnvironment()
        {
            tempDir = Directory.CreateTempSubdirectory("PythonSourceGenerator");
            tempDir.Create();
        }

        public void Dispose()
        {
            Directory.Delete(tempDir.FullName, true);
        }

        public string TempDir => tempDir.FullName;
    }
}
