
namespace PythonSourceGenerator.Tests
{
    public class TestEnvironment : IDisposable
    {
        private readonly string pythonLocation = PyEnv.TryLocatePython("3.10");
        private PyEnv env;
        private DirectoryInfo tempDir;

        public TestEnvironment()
        {
            tempDir = Directory.CreateTempSubdirectory("PythonSourceGenerator");
            tempDir.Create();
            env = new PyEnv(tempDir.FullName, pythonLocation);
        }

        public void Dispose()
        {
            Directory.Delete(tempDir.FullName, true);
            env.Dispose();
        }

        public string TempDir => tempDir.FullName;
    }
}
