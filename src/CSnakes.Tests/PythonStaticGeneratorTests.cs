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

        var module = ModuleReflection.MethodsFromFunctionDefinitions(functions, "test").ToImmutableArray();

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
}
