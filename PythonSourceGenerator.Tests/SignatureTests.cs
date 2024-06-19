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
        [InlineData("def hello_world(name): \n    ...\n", "object HelloWorld(object name)")]
        [InlineData("def hello_world(name: str) -> str:\n    ...\n", "string HelloWorld(string name)")]
        [InlineData("def hello_world(name: str, age: int) -> str:\n    ...\n", "string HelloWorld(string name, long age)")]
        [InlineData("def hello_world(numbers: list[float]) -> list[int]:\n    ...\n", "List<long> HelloWorld(List<double> numbers)")]
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
                var csharp = module.Compile();
                Assert.Contains(expected, csharp);
            }
        }

        [Theory]
        [InlineData("from decimal import Decimal\ndef hello_world(dec: Decimal) -> Decimal:\n    ...\n", "object HelloWorld(object dec)")]
        public void TestGeneratedSignatureFromModule(string code, string expected)
        {

            var tempName = string.Format("{0}_{1:N}", "test", Guid.NewGuid().ToString("N"));
            File.WriteAllText(Path.Combine(testEnv.TempDir, $"{tempName}.py"), code);

            using (Py.GIL())
            {
                // create a Python scope
                using PyModule scope = Py.CreateScope();
                var testObject = scope.Import(tempName);
                var module = ModuleReflection.MethodsFromModule(testObject, scope);
                var csharp = module.Compile();
                Assert.Contains(expected, csharp);
            }
        }
    }
}