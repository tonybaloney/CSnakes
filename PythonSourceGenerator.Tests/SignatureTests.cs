using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Python.Runtime;
using PythonEnvironments;
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
        [InlineData("def hello_world():\n    ...\n", "PyObject HelloWorld()")]
        [InlineData("def hello_world() -> None:\n    ...\n", "void HelloWorld()")]
        [InlineData("def hello_world(name): \n    ...\n", "PyObject HelloWorld(PyObject name)")]
        [InlineData("def hello_world(name: str) -> str:\n    ...\n", "string HelloWorld(string name)")]
        [InlineData("def hello_world(name: str) -> float:\n    ...\n", "double HelloWorld(string name)")]
        [InlineData("def hello_world(name: str) -> int:\n    ...\n", "long HelloWorld(string name)")]
        [InlineData("def hello_world(name: str, age: int) -> str:\n    ...\n", "string HelloWorld(string name, long age)")]
        [InlineData("def hello_world(numbers: list[float]) -> list[int]:\n    ...\n", "IEnumerable<long> HelloWorld(List<double> numbers)")]
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

                // Check that the sample C# code compiles
                string compiledCode = PythonStaticGenerator.FormatClassFromMethods("Python.Generated.Tests", "TestClass", module);
                var tree = CSharpSyntaxTree.ParseText(compiledCode);
                var compilation = CSharpCompilation.Create("HelloWorld", options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                    .AddReferences(MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location))
                    .AddReferences(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location))
                    .AddReferences(MetadataReference.CreateFromFile(typeof(Py).Assembly.Location))
                    .AddReferences(MetadataReference.CreateFromFile(typeof(PythonEnvironments.PythonEnvironment).Assembly.Location))
                    .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "netstandard").Location)) // TODO: Ensure 2.0
                    .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.Runtime").Location)) 
                    .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.Collections").Location)) 
                    .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.Linq.Expressions").Location)) 

                    .AddSyntaxTrees(tree);
                var result = compilation.Emit(testEnv.TempDir + "/HelloWorld.dll");
                Assert.True(result.Success, compiledCode + "\n" + string.Join("\n", result.Diagnostics));
            }
        }

        [Theory]
        [InlineData("from decimal import Decimal\ndef hello_world(dec: Decimal) -> Decimal:\n    ...\n", "PyObject HelloWorld(PyObject dec)")]
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