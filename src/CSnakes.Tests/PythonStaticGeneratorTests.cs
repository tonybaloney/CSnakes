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
        string compiledCode = PythonStaticGenerator.FormatClassFromMethods("Python.Generated.Tests", "TestClass", module, "test", functions, sourceText.GetContentHash());

        try
        {
            compiledCode.ShouldMatchApproved(options =>
                options.WithDiscriminator(// Just keep last part of the dotted name, e.g.:
                                          // "CSnakes.Tests.python.test_args.py" -> "test_args"
                                          Path.GetFileNameWithoutExtension(resourceName).Split('.').Last())
                       .SubFolder(GetType().Name)
                       .WithFilenameGenerator((info, d, type, ext) => $"{info.MethodName}{d}.{type}.{ext}")
                       .NoDiff());
        }
        catch (FileNotFoundException ex) when (ex.FileName is { } fn
                                               && fn.Contains(".received.", StringComparison.OrdinalIgnoreCase))
        {
            // `ShouldMatchApproved` deletes the received file when the condition is met:
            // https://github.com/shouldly/shouldly/blob/4.2.1/src/Shouldly/ShouldlyExtensionMethods/ShouldMatchApprovedTestExtensions.cs#L70
            //
            // `File.Delete` is documented to never throw an exception if the file doesn't exist:
            //
            // > If the file to be deleted does not exist, no exception is thrown. Source:
            // > https://learn.microsoft.com/en-us/dotnet/api/system.io.file.delete?view=net-8.0#remarks
            //
            // However, `FileNotFoundException` has been observed on some platforms during CI runs
            // so we catch it and should be harmless to ignore.
        }
    }
}
