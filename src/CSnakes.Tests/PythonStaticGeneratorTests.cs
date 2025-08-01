using CSnakes.Parser;
using CSnakes.Reflection;
using Microsoft.CodeAnalysis.Text;
using Shouldly;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CSnakes.Tests;

public class PythonStaticGeneratorTests
{
    private static Assembly Assembly => typeof(GeneratedSignatureTests).Assembly;

    public static readonly TheoryData<string> ResourceNames =
        new(from name in Assembly.GetManifestResourceNames()
            where name.EndsWith(".py")
            select name);

    [Theory]
    [MemberData(nameof(ResourceNames))]
    public void FormatClassFromMethods(string resourceName)
    {
        SourceText sourceText;

        using (var stream = Assembly.GetManifestResourceStream(resourceName))
        {
            Assert.NotNull(stream);
            using var reader = new StreamReader(stream);
            string normalizedText = Regex.Replace(reader.ReadToEnd(), @"\r?\n", "\n");
            sourceText = SourceText.From(normalizedText);
        }

        _ = PythonParser.TryParseFunctionDefinitions(sourceText, out var functions, out var errors);
        Assert.Empty(errors);

        var module = ModuleReflection.MethodsFromFunctionDefinitions(functions).ToImmutableArray();

        // Just keep last part of the dotted name, e.g.:
        // "CSnakes.Tests.python.test_args.py" -> "test_args"
        var nameDiscriminator = Path.GetFileNameWithoutExtension(resourceName).Split('.').Last();

        string compiledCode = PythonStaticGenerator.FormatClassFromMethods("Python.Generated.Tests", "TestClass", module, "test", functions, sourceText,
                                                                           embedSourceText: nameDiscriminator.Equals("test_source", StringComparison.OrdinalIgnoreCase));

        compiledCode.ShouldMatchApproved(options =>
            options.WithDiscriminator(nameDiscriminator)
                   .SubFolder(GetType().Name)
                   .WithFilenameGenerator((info, d, type, ext) => $"{info.MethodName}{d}.{type}.{ext}")
                   .NoDiff());
    }

    [Theory]
    [InlineData("/tmp/example.py", "", "CSnakes.Runtime", "Example", "CSnakes.Runtime.Example.py.g.cs", "example")]
    [InlineData("/tmp/example.py", "tmp", "CSnakes.Runtime", "Example", "CSnakes.Runtime.Example.py.g.cs", "example")]
    [InlineData("/tmp/submodule/example.py", "tmp", "CSnakes.Runtime.Submodule", "Example", "CSnakes.Runtime.Submodule.Example.py.g.cs", "submodule.example")]
    [InlineData("/tmp/another_example.py", "", "CSnakes.Runtime", "AnotherExample", "CSnakes.Runtime.AnotherExample.py.g.cs", "another_example")]
    [InlineData("/tmp/submodule/__init__.py", "tmp", "CSnakes.Runtime", "Submodule", "CSnakes.Runtime.Submodule.py.g.cs", "submodule")]
    [InlineData("/tmp/submodule/another_example.py", "tmp", "CSnakes.Runtime.Submodule", "AnotherExample", "CSnakes.Runtime.Submodule.AnotherExample.py.g.cs", "submodule.another_example")]
    [InlineData("/tmp/submodule/foo/__init__.py", "tmp", "CSnakes.Runtime.Submodule", "Foo", "CSnakes.Runtime.Submodule.Foo.py.g.cs", "submodule.foo")]
    [InlineData("/tmp/submodule/bar/__init__.py", "tmp/submodule", "CSnakes.Runtime", "Bar", "CSnakes.Runtime.Bar.py.g.cs", "bar")]
    public void VerifySimpleNamespace(string path, string root, string expectedNamespace, string expectedClass, string expectedFileName, string expectedModuleAbsoluteName)
    {
        var names = PythonStaticGenerator.GetNamespaceAndClassName(path, root);
        names.Namespace.ShouldBe(expectedNamespace);
        names.PascalFileName.ShouldBe(expectedClass);
        names.GeneratedFileName.ShouldBe(expectedFileName);
        names.ModuleAbsoluteName.ShouldBe(expectedModuleAbsoluteName);
    }
}
