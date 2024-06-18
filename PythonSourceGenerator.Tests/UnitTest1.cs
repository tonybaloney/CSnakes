using Microsoft.CodeAnalysis.CSharp.Syntax;
using Python.Runtime;
using PythonEnvironments;
using PythonSourceGenerator.Reflection;

namespace PythonSourceGenerator.Tests
{
    public class BasicSmokeTest
    {
        PythonEnvironment env;
        DirectoryInfo tempDir;

        public BasicSmokeTest()
        {
            tempDir = Directory.CreateTempSubdirectory("PythonSourceGenerator");
            tempDir.Create();
            env = new PythonEnvironment(tempDir.FullName);
        }

        ~BasicSmokeTest()
        {
            tempDir.Delete(true);
        }

        [Fact]
        public void TestHelloWorld()
        {
            var code = @"def hello_world(name: str) -> str:
    ...
";
            File.WriteAllText(Path.Combine(tempDir.FullName, "test_r11.py"), code);

            using (Py.GIL())
            {
                // create a Python scope
                using (PyModule scope = Py.CreateScope())
                {
                    var testObject = scope.Import("test_r11");
                    var module = ModuleReflection.MethodsFromModule(testObject, scope);
                    Assert.Single(module);
                    var method = module[0];
                    Assert.Equal("HelloWorld", method.Identifier.Text);

                    var csharp = module.Compile();
                    Assert.Contains("public static string HelloWorld(string name)", csharp);
                }
            }
        }
    }
}