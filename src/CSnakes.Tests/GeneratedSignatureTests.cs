using CSnakes.Parser;
using CSnakes.Reflection;
using CSnakes.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.ComponentModel;

namespace CSnakes.Tests;

public class GeneratedSignatureTests
{
    [Theory]
    [InlineData("def hello_world():\n    ...\n", "PyObject HelloWorld()")]
    [InlineData("def hello_world() -> None:\n    ...\n", "void HelloWorld()")]
    [InlineData("def hello_world(name): \n    ...\n", "PyObject HelloWorld(PyObject name)")]
    [InlineData("def hello_world(new): \n    ...\n", "PyObject HelloWorld(PyObject @new)")]
    [InlineData("def hello_world(name: str) -> str:\n    ...\n", "string HelloWorld(string name)")]
    [InlineData("def hello_world(name: str) -> float:\n    ...\n", "double HelloWorld(string name)")]
    [InlineData("def hello_world(name: str) -> int:\n    ...\n", "long HelloWorld(string name)")]
    [InlineData("def hello_world(name: str, age: int) -> str:\n    ...\n", "string HelloWorld(string name, long age)")]
    [InlineData("def hello_world(numbers: list[float]) -> list[int]:\n    ...\n", "IReadOnlyList<long> HelloWorld(IReadOnlyList<double> numbers)")]
    [InlineData("def hello_world(numbers: List[float]) -> List[int]:\n    ...\n", "IReadOnlyList<long> HelloWorld(IReadOnlyList<double> numbers)")]
    [InlineData("def hello_world(numbers: Sequence[float]) -> typing.Sequence[int]:\n    ...\n", "IReadOnlyList<long> HelloWorld(IReadOnlyList<double> numbers)")]
    [InlineData("def hello_world(value: tuple[int]) -> None:\n    ...\n", "void HelloWorld(ValueTuple<long> value)")]
    [InlineData("def hello_world(a: bool, b: str, c: list[tuple[int, float]]) -> bool: \n ...\n", "bool HelloWorld(bool a, string b, IReadOnlyList<(long, double)> c)")]
    [InlineData("def hello_world(a: bool = True, b: str = None) -> bool: \n ...\n", "bool HelloWorld(bool a = true, string? b = null)")]
    [InlineData("def hello_world(a: bytes, b: bool = False, c: float = 0.1) -> None: \n ...\n", "void HelloWorld(byte[] a, bool b = false, double c = 0.1)")]
    [InlineData("def hello_world(a: str = 'default') -> None: \n ...\n", "void HelloWorld(string a = \"default\")")]
    [InlineData("def hello_world(a: str, *args) -> None: \n ...\n", "void HelloWorld(string a, PyObject[]? args = null)")]
    [InlineData("def hello_world(a: str, *, b: int) -> None: \n ...\n", "void HelloWorld(string a, long b)")]
    [InlineData("def hello_world(a: str, *, b: int = 3) -> None: \n ...\n", "void HelloWorld(string a, long b = 3)")]
    [InlineData("def hello_world(a: str, *args, **kwargs) -> None: \n ...\n", "void HelloWorld(string a, PyObject[]? args = null, IReadOnlyDictionary<string, PyObject>? kwargs = null)")]
    [InlineData("def hello(a: int = 0xdeadbeef) -> None:\n ...\n", "void Hello(long a = 0xDEADBEEF)")]
    [InlineData("def hello(a: int = 0b10101010) -> None:\n ...\n", "void Hello(long a = 0b10101010)")]
    [InlineData("def hello(a: int = 2147483648) -> None:\n ...\n", "void Hello(long a = 2147483648L)")]
    [InlineData("def hello(a: Optional[int] = None, b: typing.Optional[int] = None) -> None:\n ...\n", "void Hello(long? a = null, long? b = null)")]
    [InlineData("def hello(a: typing.List[int], b: typing.Dict[str, int]) -> typing.Tuple[str, int]:\n ...\n", "(string, long) Hello(IReadOnlyList<long> a, IReadOnlyDictionary<string, long> b)")]
    [InlineData("def hello(a: Dict[int, str]) -> typing.Dict[str, int]:\n ...\n", "IReadOnlyDictionary<string, long> Hello(IReadOnlyDictionary<long, string> a)")]
    [InlineData("def hello(a: Mapping[int, str]) -> typing.Mapping[str, int]:\n ...\n", "IReadOnlyDictionary<string, long> Hello(IReadOnlyDictionary<long, string> a)")]
    [InlineData("def hello() -> Generator[int, str, bool]:\n ...\n", "IGeneratorIterator<long, string, bool> Hello()")]
    [InlineData("def hello() -> typing.Generator[int, str, bool]:\n ...\n", "IGeneratorIterator<long, string, bool> Hello()")]
    [InlineData("def hello() -> Buffer:\n ...\n", "IPyBuffer Hello()")]
    public void TestGeneratedSignature(string code, string expected)
    {
        SourceText sourceText = SourceText.From(code);

        // create a Python scope
        PythonParser.TryParseFunctionDefinitions(sourceText, out var functions, out var errors);
        Assert.Empty(errors);
        var module = ModuleReflection.MethodsFromFunctionDefinitions(functions, "test").ToImmutableArray();
        var method = Assert.Single(module);
        Assert.Equal($"public {expected}", method.Syntax.WithBody(null).NormalizeWhitespace().ToString());

        // Check that the sample C# code compiles
        string compiledCode = PythonStaticGenerator.FormatClassFromMethods("Python.Generated.Tests", "TestClass", module, "test", functions, sourceText.GetContentHash());
        var tree = CSharpSyntaxTree.ParseText(compiledCode);
        var compilation = CSharpCompilation.Create("HelloWorld", options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(IList<>).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(TypeConverter).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(IReadOnlyDictionary<,>).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(IPythonEnvironmentBuilder).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(ILogger<>).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "netstandard").Location)) // TODO: (track) Ensure 2.0
            .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.Runtime").Location))
            .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.Collections").Location))
            .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.ComponentModel").Location))
            .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.Linq.Expressions").Location))

            .AddSyntaxTrees(tree);
        var result = compilation.Emit(Stream.Null);
        // TODO : Log compiler warnings.
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList().ForEach(d => Assert.Fail(d.ToString()));
        Assert.True(result.Success, compiledCode + "\n" + string.Join("\n", result.Diagnostics));
    }
}
