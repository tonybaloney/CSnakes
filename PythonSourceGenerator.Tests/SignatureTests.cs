using Python.Runtime;
using PythonSourceGenerator.Reflection;

namespace PythonSourceGenerator.Tests
{
    public class BasicSmokeTest : IClassFixture<TestEnvironment>
    {
        TestEnvironment testEnv;

        public BasicSmokeTest(TestEnvironment testEnv)
        {
            this.testEnv = testEnv;
        }

        [Theory]
        [InlineData("def hello_world():\n    ...\n", "object HelloWorld()")]
        [InlineData("def hello_world() -> None:\n    ...\n", "void HelloWorld()")]
        [InlineData("def hello_world(name: str) -> str:\n    ...\n", "string HelloWorld(string name)")]
        [InlineData("def hello_world(name: str, age: int) -> str:\n    ...\n", "string HelloWorld(string name, long age)")]
        public void TestGeneratedSignature(string code, string expected)
        {
            
            var tempName = string.Format("{0}_{1:N}", "test", Guid.NewGuid().ToString("N"));
            File.WriteAllText(Path.Combine(testEnv.TempDir, $"{tempName}.py"), code);

            using (Py.GIL())
            {
                // create a Python scope
                using PyModule scope = Py.CreateScope();
                var testObject = scope.Import(tempName);
                var module = ModuleReflection.MethodsFromModule(testObject, scope);
                Assert.Single(module);
                var csharp = module.Compile();
                Assert.Contains(expected, csharp);
            }
        }
    }
}