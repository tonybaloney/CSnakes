using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Python.Runtime;
using PythonSourceGenerator.Parser;
using PythonSourceGenerator.Reflection;
using System.Reflection;

namespace PythonSourceGenerator.Tests;

public class IntegrationTests : IClassFixture<TestEnvironment>
{
    TestEnvironment testEnv;

    public IntegrationTests(TestEnvironment testEnv)
    {
        this.testEnv = testEnv;
    }

    private bool Compile(string code, string assemblyName)
    {
        var tempName = string.Format("{0}_{1:N}", "test", Guid.NewGuid().ToString("N"));
        File.WriteAllText(Path.Combine(testEnv.TempDir, $"{tempName}.py"), code);

        // create a Python scope
        PythonSignatureParser.TryParseFunctionDefinitions(code, out var functions, out var errors);

        var module = ModuleReflection.MethodsFromFunctionDefinitions(functions, "test");
        var csharp = module.Select(m => m.Syntax).Compile();

        // Check that the sample C# code compiles
        string compiledCode = PythonStaticGenerator.FormatClassFromMethods("Python.Generated.Tests", "TestClass", module);
        var tree = CSharpSyntaxTree.ParseText(compiledCode);
        var compilation = CSharpCompilation.Create(assemblyName, options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(IEnumerable<>).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(IReadOnlyDictionary<,>).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(PythonEnvironments.PythonEnvironment).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(Py).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "netstandard").Location)) // TODO: Ensure 2.0
            .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.Runtime").Location))
            .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.Collections").Location))
            .AddReferences(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "System.Linq.Expressions").Location))

            .AddSyntaxTrees(tree);
        var path = testEnv.TempDir + $"/{assemblyName}.dll";
        var result = compilation.Emit(path);
        Assert.True(result.Success, compiledCode + "\n" + string.Join("\n", result.Diagnostics));
        // Delete assembly
        File.Delete(path);
        return result.Success;
    }

    [Fact]
    public void TestBasicString()
    {
        var code = """
def foo(in_: str) -> str:
    return in_.upper()
""";
        Assert.True(Compile(code, "stringFoo"));
    }
}
