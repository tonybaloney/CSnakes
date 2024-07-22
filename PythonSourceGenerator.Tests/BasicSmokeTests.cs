using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Python.Runtime;
using PythonSourceGenerator.Parser;
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
        [InlineData("def hello_world(new): \n    ...\n", "PyObject HelloWorld(PyObject @new)")]
        [InlineData("def hello_world(name: str) -> str:\n    ...\n", "string HelloWorld(string name)")]
        [InlineData("def hello_world(name: str) -> float:\n    ...\n", "double HelloWorld(string name)")]
        [InlineData("def hello_world(name: str) -> int:\n    ...\n", "long HelloWorld(string name)")]
        [InlineData("def hello_world(name: str, age: int) -> str:\n    ...\n", "string HelloWorld(string name, long age)")]
        [InlineData("def hello_world(numbers: list[float]) -> list[int]:\n    ...\n", "IEnumerable<long> HelloWorld(IEnumerable<double> numbers)")]
        [InlineData("def hello_world(numbers: List[float]) -> List[int]:\n    ...\n", "IEnumerable<long> HelloWorld(IEnumerable<double> numbers)")]
        [InlineData("def hello_world(a: bool, b: str, c: list[tuple[int, float]]) -> bool: \n ...\n", "bool HelloWorld(bool a, string b, IEnumerable<(long, double)> c)")]
        [InlineData("def hello_world(a: bool = True, b: str = None) -> bool: \n ...\n", "bool HelloWorld(bool a = true, string b = null)")]
        [InlineData("def hello_world(a: str, *args) -> None: \n ...\n", "void HelloWorld(string a, ValueTuple<PyObject> args)")]
        [InlineData("def hello_world(a: str, *, b: int) -> None: \n ...\n", "void HelloWorld(string a, ValueTuple<PyObject> args, long b)")]
        [InlineData("def hello_world(a: str, *, b: int = 3) -> None: \n ...\n", "void HelloWorld(string a, ValueTuple<PyObject> args, long b = 3)")]
        [InlineData("def hello_world(a: str, *args, **kwargs) -> None: \n ...\n", "void HelloWorld(string a, ValueTuple<PyObject> args, IReadOnlyDictionary<string, PyObject> kwargs)")]
        [InlineData("def hello_world(a: str, *args) -> None: \n ...\n", "void HelloWorld(string a, Tuple<PyObject> args)")]
        [InlineData("def hello(a: int = 0xdeadbeef) -> None:\n ...\n", "void Hello(long a = 0xDEADBEEF)")]
        [InlineData("def hello(a: int = 0b10101010) -> None:\n ...\n", "void Hello(long a = 0b10101010)")]
        [InlineData("def hello_world(a: str, *, b: int) -> None: \n ...\n", "void HelloWorld(string a, Tuple<PyObject> args, long b)")]
        [InlineData("def hello_world(a: str, *, b: int = 3) -> None: \n ...\n", "void HelloWorld(string a, Tuple<PyObject> args, long b = 3)")]
        [InlineData("def hello_world(a: str, *args, **kwargs) -> None: \n ...\n", "void HelloWorld(string a, Tuple<PyObject> args, IReadOnlyDictionary<string, PyObject> kwargs)")]
        public void TestGeneratedSignature(string code, string expected)
        {
            
            var tempName = string.Format("{0}_{1:N}", "test", Guid.NewGuid().ToString("N"));
            File.WriteAllText(Path.Combine(testEnv.TempDir, $"{tempName}.py"), code);

            // create a Python scope
            PythonSignatureParser.TryParseFunctionDefinitions(code, out var functions, out var errors);
            Assert.Empty(errors);
            var module = ModuleReflection.MethodsFromFunctionDefinitions(functions, "test");
                var csharp = module.Select(m => m.Syntax).Compile();
                Assert.Contains(expected, csharp);

                // Check that the sample C# code compiles
                string compiledCode = PythonStaticGenerator.FormatClassFromMethods("Python.Generated.Tests", "TestClass", module);
                var tree = CSharpSyntaxTree.ParseText(compiledCode);
                var compilation = CSharpCompilation.Create("HelloWorld", options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                    .AddReferences(MetadataReference.CreateFromFile(typeof(IEnumerable<>).Assembly.Location))
                    .AddReferences(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location))
                    .AddReferences(MetadataReference.CreateFromFile(typeof(IReadOnlyDictionary<,>).Assembly.Location))
                    .AddReferences(MetadataReference.CreateFromFile(typeof(PythonEnvironments.PythonEnvironment).Assembly.Location))
                    .AddReferences(MetadataReference.CreateFromFile(typeof(Py).Assembly.Location))
                    .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "netstandard").Location)) // TODO: (track) Ensure 2.0
                    .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.Runtime").Location)) 
                    .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.Collections").Location)) 
                    .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.Linq.Expressions").Location)) 

                    .AddSyntaxTrees(tree);
                var result = compilation.Emit(testEnv.TempDir + "/HelloWorld.dll");
                Assert.True(result.Success, compiledCode + "\n" + string.Join("\n", result.Diagnostics));
        }
    }
}