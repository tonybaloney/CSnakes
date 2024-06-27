
using PythonSourceGenerator;

namespace PythonSourceGenerator.Tests
{
    public class TestEnvironment : IDisposable
    {
        private IPythonEnvironment env;
        private DirectoryInfo tempDir;

        public TestEnvironment()
        {
            tempDir = Directory.CreateTempSubdirectory("PythonSourceGenerator");
            tempDir.Create();
            env = new PythonEnvironment(tempDir.FullName, "3.10").Build();
        }

        public void Dispose()
        {
            Directory.Delete(tempDir.FullName, true);
            env.Dispose();
        }

        public string TempDir => tempDir.FullName;
    }
}
