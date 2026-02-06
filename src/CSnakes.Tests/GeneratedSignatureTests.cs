using Basic.Reference.Assemblies;
using CSnakes.Parser;
using CSnakes.Parser.Types;
using CSnakes.Reflection;
using CSnakes.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

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
    [InlineData("def hello_world(a: bool = True, b: Optional[str] = None) -> bool: \n ...\n", "bool HelloWorld(bool a = true, string? b = null)")]
    [InlineData("def hello_world(a: bool = True, b: str | None = None) -> bool: \n ...\n", "bool HelloWorld(bool a = true, string? b = null)")]
    [InlineData("def hello_world(a: Optional[int], b: Optional[str]) -> Optional[bool]: \n ...\n", "bool? HelloWorld(long? a, string? b)")]
    [InlineData("def hello_world(a: int | None, b: str | None) -> bool | None: \n ...\n", "bool? HelloWorld(long? a, string? b)")]
    [InlineData("def hello_world(a: bytes, b: bool = False, c: float = 0.1) -> None: \n ...\n", "void HelloWorld(ReadOnlySpan<byte> a, bool b = false, double c = 0.1)")]
    [InlineData("def hello_world(a: bytes) -> bytes: \n ...\n", "byte[] HelloWorld(ReadOnlySpan<byte> a)")]
    [InlineData("def hello_world(a: bytes = b'hello') -> bytes: \n ...\n", "byte[] HelloWorld(byte[]? a = null)")]
    [InlineData("def hello_world(a: bytes | None) -> bytes: \n ...\n", "byte[] HelloWorld(byte[]? a)")]
    [InlineData("async def hello_world(a: bytes, b: bool = False, c: float = 0.1) -> None: \n ...\n", "Task HelloWorld(ReadOnlySpan<byte> a, bool b = false, double c = 0.1, CancellationToken cancellationToken = default)")]
    [InlineData("async def hello_world(a: bytes) -> bytes: \n ...\n", "Task<byte[]> HelloWorld(ReadOnlySpan<byte> a, CancellationToken cancellationToken = default)")]
    [InlineData("def hello_world(a: str = 'default') -> None: \n ...\n", "void HelloWorld(string a = \"default\")")]
    [InlineData("def hello_world(a: str, *args) -> None: \n ...\n", "void HelloWorld(string a, ReadOnlySpan<PyObject> args = default)")]
    [InlineData("def hello_world(a: str, *, b: int) -> None: \n ...\n", "void HelloWorld(string a, long b)")]
    [InlineData("def hello_world(a: str, *, b: int = 3) -> None: \n ...\n", "void HelloWorld(string a, long b = 3)")]
    [InlineData("def hello_world(a: str, *args, **kwargs) -> None: \n ...\n", "void HelloWorld(string a, ReadOnlySpan<PyObject> args = default, ReadOnlySpan<KeywordArg> kwargs = default)")]
    [InlineData("def hello(a: int = 0xdeadbeef) -> None:\n ...\n", "void Hello(long a = 0xDEADBEEF)")]
    [InlineData("def hello(a: int = 0b10101010) -> None:\n ...\n", "void Hello(long a = 0b10101010)")]
    [InlineData("def hello(a: int = 0o777) -> None:\n ...\n", "void Hello(long a = 0x1FF)")]
    [InlineData("def hello(a: int = 2147483648) -> None:\n ...\n", "void Hello(long a = 2147483648L)")]
    [InlineData("def hello(a: Optional[int] = None, b: typing.Optional[int] = None, c: int | None = None, d: None | int = None) -> None:\n ...\n", "void Hello(long? a = null, long? b = null, long? c = null, long? d = null)")]
    [InlineData("def hello(a: typing.List[int], b: typing.Dict[str, int]) -> typing.Tuple[str, int]:\n ...\n", "(string, long) Hello(IReadOnlyList<long> a, IReadOnlyDictionary<string, long> b)")]
    [InlineData("def hello(a: Dict[int, str]) -> typing.Dict[str, int]:\n ...\n", "IReadOnlyDictionary<string, long> Hello(IReadOnlyDictionary<long, string> a)")]
    [InlineData("def hello(a: Mapping[int, str]) -> typing.Mapping[str, int]:\n ...\n", "IReadOnlyDictionary<string, long> Hello(IReadOnlyDictionary<long, string> a)")]
    [InlineData("def hello() -> Generator[int, str, bool]:\n ...\n", "IGeneratorIterator<long, string, bool> Hello()")]
    [InlineData("def hello() -> typing.Generator[int, str, bool]:\n ...\n", "IGeneratorIterator<long, string, bool> Hello()")]
    [InlineData("def hello() -> Buffer:\n ...\n", "IPyBuffer Hello()")]
    [InlineData("def hello(data: Literal[1, 'two', 3.0]) -> None:\n ...\n", "void Hello(PyObject data)")]
    [InlineData("def hello(n: None = None) -> None:\n ...\n", "void Hello(PyObject? n = null)")]
    [InlineData("def hello(val: bytes = b'hello', /) -> None:\n ...\n", "void Hello(byte[]? val = null)")]
    [InlineData("def hello(val: str = u'world', /) -> None:\n ...\n", "void Hello(string val = \"world\")")]
    [InlineData("def hello() -> Awaitable[int]:\n ...\n", "IAwaitable<long> Hello()")]
    [InlineData("async def hello() -> None:\n ...\n", "Task Hello(CancellationToken cancellationToken = default)")]
    [InlineData("async def hello():\n ...\n", "Task<PyObject> Hello(CancellationToken cancellationToken = default)")]
    [InlineData("def hello(n: Foo = ...) -> None:\n ...\n", "void Hello(PyObject? n = null)")]
    [InlineData("def hello(a: str, b: int = 4, *, kw: str) -> None:\n ...\n", "void Hello(string a, string kw, long b = 4)")]
    [InlineData("def hello() -> str | int: \n  ...\n", "PyObject Hello()")]
    [InlineData("def hello() -> Annotated[object, 'c#:FooBar']:\n ...\n", "FooBar Hello()")]
    [InlineData("def escape(s: str, quote: bool = True) -> str: ...\n", "string Escape(string s, bool quote = true)")]
    [InlineData("""

        # csharp: ignore
        def foo(bar):
            pass

        """,
        "PyObject Foo(PyObject bar)",
        Label = "C# ignore comment on line above")]
    [InlineData("""

        def foo(
            bar  # csharp: ignore
        ):
            pass

        """,
        "PyObject Foo(PyObject bar)",
        Label = "C# ignore comment in parameter list")]
    [InlineData("""

        def foo(bar):
            # csharp: ignore
            pass

        """,
        "PyObject Foo(PyObject bar)",
        Label = "C# ignore comment inside function body")]
    public void TestGeneratedSignature(string code, string expected)
    {
        SourceText sourceText = SourceText.From(code);
        Assert.True(PythonParser.TryParseFunctionDefinitions(sourceText, out var functions, out var errors));
        Assert.Empty(errors);
        var module = ModuleReflection.MethodsFromFunctionDefinitions(functions).ToImmutableArray();
        var method = Assert.Single(module);
        CompileAndVerifyCode(module, functions, sourceText);
        Assert.Equal($"public {expected}", method.Syntax.WithBody(null).NormalizeWhitespace().ToString());
    }

    [Theory]
    [InlineData("def t() -> Match[str] | None:\n ...\n")]
    [InlineData("def t(pattern: str | Pattern[str]) -> Match[str] | None:\n ...\n")]
    public void TestGeneratedSignatureCompiledUnions(string code)
    {
        SourceText sourceText = SourceText.From(code);
        Assert.True(PythonParser.TryParseFunctionDefinitions(sourceText, out var functions, out var errors));
        Assert.Empty(errors);
        var module = ModuleReflection.MethodsFromFunctionDefinitions(functions).ToImmutableArray();
        CompileAndVerifyCode(module, functions, sourceText);
    }

    private static void CompileAndVerifyCode(ImmutableArray<MethodDefinition> methods, PythonFunctionDefinition[] functions, SourceText sourceText)
    {
        string compiledCode = PythonStaticGenerator.FormatClassFromMethods("Python.Generated.Tests", "TestClass", methods, "test", functions, sourceText);
        var tree = CSharpSyntaxTree.ParseText(compiledCode, cancellationToken: TestContext.Current.CancellationToken);
        var compilation = CSharpCompilation.Create("HelloWorld", options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
#if NET8_0
            .WithReferenceAssemblies(ReferenceAssemblyKind.Net80)
#elif NET9_0
            .WithReferences(Net90.References.All)
#else
#error Unsupported .NET tareget
#endif
            .AddReferences(MetadataReference.CreateFromFile(typeof(IPythonEnvironmentBuilder).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(ILogger<>).Assembly.Location))
            .AddSyntaxTrees(tree,
                            CSharpSyntaxTree.ParseText(path: "FooBar.cs", cancellationToken: TestContext.Current.CancellationToken, text: """
                                using System;
                                using CSnakes.Linq;

                                public class FooBar : IPyObjectReadable<FooBar>
                                {
                                    public static IPyObjectReader<FooBar> Reader => throw new NotImplementedException();
                                }
                                """));
        var result = compilation.Emit(Stream.Null, cancellationToken: TestContext.Current.CancellationToken);
        // TODO : Log compiler warnings.
        result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList().ForEach(d => Assert.Fail(d.ToString()));
        Assert.True(result.Success, compiledCode + "\n" + string.Join("\n", result.Diagnostics));
    }

    [Theory]
    // Same parameter types, different defaults.
    [InlineData(
        "def x(future: None = None, /, *, depth: int = 1, limit: int | None = None) -> None:\n ...",
        "def x(future: Future[Any], /, *, depth: int = 1, limit: int | None = None) -> None:\n ...")]
    // Same parameter types, different names
    [InlineData(
        "def x(foo: int, bar: str = 'default') -> None:\n ...",
        "def x(foo_2: int, bar: str = 'default') -> None:\n ...")]
    // Same parameter types, different defaults and names
    [InlineData(
        "def x(foo: int, bar: str = 'default1') -> None:\n ...",
        "def x(foo_2: int, bar: str = 'default2') -> None:\n ...")]
    // Same parameter types, one with a default, one without
    [InlineData(
        "def x(foo: int = 5) -> None:\n ...",
        "def x(foo: int) -> None:\n ...")]
    [InlineData(
        "def x(foo: Optional[FooBar]) -> None:\n ...",
        "def x(foo: FooBar) -> None:\n ...")]
    [InlineData(
        "def x(foo: str) -> None:\n ...",
        "def x(foo: Optional[str]) -> None:\n ...")]
    public void TestMethodEquivalence(string code1, string code2)
    {
        SourceText sourceText1 = SourceText.From(code1);
        SourceText sourceText2 = SourceText.From(code2);

        // create a Python scope
        Assert.True(PythonParser.TryParseFunctionDefinitions(sourceText1, out var functions1, out var errors1));
        Assert.Empty(errors1);
        Assert.True(PythonParser.TryParseFunctionDefinitions(sourceText2, out var functions2, out var errors2));
        Assert.Empty(errors2);
        var module1 = ModuleReflection.MethodsFromFunctionDefinitions(functions1).ToImmutableArray();
        var method1 = Assert.Single(module1);
        var module2 = ModuleReflection.MethodsFromFunctionDefinitions(functions2).ToImmutableArray();
        var method2 = Assert.Single(module2);

        var comparator = new MethodDefinitionComparator();
        Assert.True(comparator.Equals(method1, method2));
        Assert.Single(new[] { method1, method2 }.Distinct(comparator));
    }

    [Theory]
    [InlineData("def x(a: str) -> str:\n ...", "def x(a: int) -> int:\n ...")]
    [InlineData("def x(a: float) -> float:\n ...", "def x(a: Any) -> float:\n ...")]
    [InlineData("def x(a: str) -> str:\n ...", "def x(a: str, b: int) -> str:\n ...")]
    public void TestMethodInequivalence(string code1, string code2)
    {
        SourceText sourceText1 = SourceText.From(code1);
        SourceText sourceText2 = SourceText.From(code2);
        // create a Python scope
        Assert.True(PythonParser.TryParseFunctionDefinitions(sourceText1, out var functions1, out var errors1));
        Assert.Empty(errors1);
        Assert.True(PythonParser.TryParseFunctionDefinitions(sourceText2, out var functions2, out var errors2));
        Assert.Empty(errors2);
        var module1 = ModuleReflection.MethodsFromFunctionDefinitions(functions1).ToImmutableArray();
        var method1 = Assert.Single(module1);
        var module2 = ModuleReflection.MethodsFromFunctionDefinitions(functions2).ToImmutableArray();
        var method2 = Assert.Single(module2);

        var comparator = new MethodDefinitionComparator();
        Assert.False(comparator.Equals(method1, method2));
        Assert.Equal(2, new[] { method1, method2 }.Distinct(comparator).Count());
    }

    [Fact]
    public void TestSimpleUnionOverloads()
    {
        SourceText code = SourceText.From("def hello(a: Union[int, str] = 5) -> Any:\n ...\n");

        // create a Python scope
        Assert.True(PythonParser.TryParseFunctionDefinitions(code, out var functions1, out var errors1));
        Assert.Empty(errors1);

        var module1 = ModuleReflection.MethodsFromFunctionDefinitions(functions1).ToImmutableArray();
        Assert.Equal(2, module1.Length);
    }

    [Fact]
    public void TestMultipleUnknownUnionOverloads()
    {
        // Verify that multiple unknown types are reduced to a single PyObject.
        SourceText code = SourceText.From("def hello(a: Union[A, B, C]) -> Any:\n ...\n");

        // create a Python scope
        Assert.True(PythonParser.TryParseFunctionDefinitions(code, out var functions1, out var errors1));
        Assert.Empty(errors1);

        var module1 = ModuleReflection.MethodsFromFunctionDefinitions(functions1).ToImmutableArray();
        Assert.Single(module1);
    }

    [Fact]
    public void TestProductUnionOverloads()
    {
        // Verify that a product of unions is reduced to a single overload with a tuple type.
        SourceText code = SourceText.From("def hello(a: Union[int, str], b: Union[float, bool]) -> Any:\n ...\n");
        // create a Python scope
        Assert.True(PythonParser.TryParseFunctionDefinitions(code, out var functions1, out var errors1));
        Assert.Empty(errors1);
        var module1 = ModuleReflection.MethodsFromFunctionDefinitions(functions1).ToImmutableArray();
        Assert.Equal(4, module1.Length);
    }

    [Theory]
    [InlineData("Union[str, None]")]
    [InlineData("str | bytes")]
    [InlineData("Foo[str] | None")]
    public void TestUnionReturnType(string returnType)
    {
        // Verify that a union return type only generates 1 method
        SourceText code = SourceText.From($"def hello() -> {returnType}:\n ...\n");
        // create a Python scope
        Assert.True(PythonParser.TryParseFunctionDefinitions(code, out var functions1, out var errors1));
        Assert.Empty(errors1);
        var module1 = ModuleReflection.MethodsFromFunctionDefinitions(functions1).ToImmutableArray();
        Assert.Single(module1);
    }

    [Theory]
    [InlineData("""
        def foo(  # csharp: ignore
                bar,
                baz,
            ):
            pass

        """,
        Label = "C# ignore comment - Multi lined function definition")]
    [InlineData("""
        def hello(): # csharp: ignore ...
            pass

        """,
        Label = "C# ignore comment - Single line function definition")]
    [InlineData("""
        def foo():  # type: ignore # csharp: ignore
            pass

        """,
        Label = "C# ignore comment - Embedded within a comment after")]
    [InlineData("""
        def bar():  # csharp: ignore # type: ignore
            pass

        """,
        Label = "C# ignore comment - Embedded within a comment before")]
    public void TestCSharpIgnoreCommentFunction(string code)
    {
        SourceText sourceText = SourceText.From(code);
        Assert.True(PythonParser.TryParseFunctionDefinitions(sourceText, out var functions, out var errors));
        Assert.Empty(errors);
        Assert.Empty(functions);

        // Verify that the source text is not indented. For more background on
        // following assertions, see:
        // - https://github.com/tonybaloney/CSnakes/pull/702#discussion_r2365595136
        // - https://github.com/tonybaloney/CSnakes/pull/702#discussion_r2365597599

        var indentations =
            from line in sourceText.Lines
            select line.ToString()
            into line
            where line.Length > 0
            select line.TakeWhile(ch => ch == ' ').Count();
        Assert.Equal(0, indentations.Min());
    }
}
