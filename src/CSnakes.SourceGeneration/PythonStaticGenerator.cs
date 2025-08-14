using CSnakes.Parser;
using CSnakes.Parser.Types;
using CSnakes.Reflection;
using CSnakes.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
using System.Text.RegularExpressions;

// Leave as simple namespace for compability with older versions
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace CSnakes;
#pragma warning restore IDE0130 // Namespace does not match folder structure

[Generator(LanguageNames.CSharp)]
public class PythonStaticGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Get analyser config options
        var embedPythonSource = context.AnalyzerConfigOptionsProvider.Select(static (options, cancellationToken) =>
            options.GlobalOptions.TryGetValue("build_property.EmbedPythonSources", out var embedSourceSwitch)
            && embedSourceSwitch.Equals("true", StringComparison.InvariantCultureIgnoreCase));

        var pythonFilesPipeline =
            context.AdditionalTextsProvider
                   .Combine(context.AnalyzerConfigOptionsProvider)
                   .Where(static e =>
                       e is var (additionalText, analyzerConfigOptions)
                       && analyzerConfigOptions.GetOptions(additionalText) is var options
                       && options.TryGetValue("build_metadata.AdditionalFiles.SourceItemType", out var type)
                       && "python".Equals(type, StringComparison.OrdinalIgnoreCase));

        // Get directory traversal root
        var rootDirectory = context.AnalyzerConfigOptionsProvider.Select(static (options, cancellationToken) =>
            options.GlobalOptions.TryGetValue("build_property.PythonRoot", out var rootDir)
                ? rootDir
                : string.Empty); // Default to empty string

        context.RegisterSourceOutput(pythonFilesPipeline.Combine(embedPythonSource).Combine(rootDirectory), static (sourceContext, opts) =>
        {
            var ((file, _), embedSourceSwitch) = opts.Left;
            var rootDir = opts.Right;

            if (Path.GetExtension(file.Path) == ".pyi")
            {
                // Don't embed sources for .pyi files, they aren't real Python files and embedding them
                // would make no sense.
                embedSourceSwitch = false;
            }

            // Convert snake_case to PascalCase
            DerivedNames? derivedNames;
            try
            {
                derivedNames = GetNamespaceAndClassName(file.Path, rootDir);
            } catch (NamespaceNotInRootException nir)
            {
                // Skip this file if it's not in the configured namespace
                Location errorLocation = Location.Create(file.Path, TextSpan.FromBounds(0, 1), new LinePositionSpan(new LinePosition(0, 1), new LinePosition(1, 1)));
                sourceContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("PSG005", "PythonStaticGenerator", nir.Message, "PythonStaticGenerator", DiagnosticSeverity.Info, true), errorLocation));
                return;
            }
            var @namespace = derivedNames.Namespace;
            var generatedFileName = derivedNames.GeneratedFileName;
            var pascalFileName = derivedNames.PascalFileName;
            var moduleAbsoluteName = derivedNames.ModuleAbsoluteName;

            // Read the file
            var code = file.GetText(sourceContext.CancellationToken);

            if (code is null) return;

            // PEP 263 – Defining Python Source Code Encodings
            // https://peps.python.org/pep-0263/

            if (code.Lines
                    .Select(line => line.ToString().TrimStart())
                    .TakeWhile(line => line is ['#', _]) // Take as long as a comment
                    .Take(2) // Must be first or second line according to PEP 263
                    .Select(line => Regex.Match(line, @"^[ \t\f]*#.*?coding[:=][ \t]*([-_.a-zA-Z0-9]+)").Groups[0])
                    .FirstOrDefault(g => g.Success) is { Value: var encodingMagicCommentValue } &&
                !"utf-8".Equals(encodingMagicCommentValue, StringComparison.OrdinalIgnoreCase))
            {
                // TODO report diagnostic and bail out?
            }

            // Parse the Python file
            try
            {
                var result = PythonParser.TryParseFunctionDefinitions(code, out PythonFunctionDefinition[] functions, out GeneratorError[]? errors);

                foreach (var error in errors)
                {
                    // Update text span
                    Location errorLocation = Location.Create(file.Path, TextSpan.FromBounds(0, 1), new LinePositionSpan(new LinePosition(error.StartLine, error.StartColumn), new LinePosition(error.EndLine, error.EndColumn)));
                    sourceContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("PSG004", "PythonStaticGenerator", error.Message, "PythonStaticGenerator", DiagnosticSeverity.Warning, true), errorLocation));
                }

                if (result)
                {
                    var methods = ModuleReflection.MethodsFromFunctionDefinitions(functions).ToImmutableArray();
                    string source = FormatClassFromMethods(@namespace, pascalFileName, methods, moduleAbsoluteName, functions, code, embedSourceSwitch);
                    sourceContext.AddSource(generatedFileName, source);
                    sourceContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("PSG002", "PythonStaticGenerator", $"Generated {generatedFileName} from {file.Path}", "PythonStaticGenerator", DiagnosticSeverity.Info, true), Location.None));
                }
            } catch (Exception ex) {
                Location errorLocation = Location.Create(file.Path, TextSpan.FromBounds(0, 1), new LinePositionSpan(new LinePosition(0, 1), new LinePosition(1, 1)));
                sourceContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("PSG999", "PythonStaticGenerator", ex.Message, "PythonStaticGenerator", DiagnosticSeverity.Error, true), errorLocation));
            }
        });
    }

    /// <summary>
    /// Get the file path after the relative path.
    /// .e.g. if the absolute path is "/tmp/foo/bar/baz.py" and the relative path is "foo/bar",
    /// it will return "baz.py".
    /// </summary>
    private static string? GetPathAfter(string absolutePath, string relativePath)
    {
        // Normalize and split paths
        var absParts = Path.GetFullPath(absolutePath).Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var relParts = relativePath.Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            .Where(p => !string.IsNullOrEmpty(p))
            .ToArray();

        // Find the index where relParts matches a subsequence in absParts
        var matchIndex = absParts
            .Select((_, i) => i)
            .FirstOrDefault(i => i + relParts.Length <= absParts.Length &&
                relParts.AsSpan().SequenceEqual(absParts.AsSpan(i, relParts.Length)));

        // If not found, return null
        if (matchIndex == 0 && (relParts.Length == 0 || !relParts.AsSpan().SequenceEqual(absParts.AsSpan(0, relParts.Length))))
            return null;

        // Return the parts after the match
        var afterParts = absParts.Skip(matchIndex + relParts.Length);
        return string.Join(Path.DirectorySeparatorChar.ToString(), afterParts);
    }

    public static DerivedNames GetNamespaceAndClassName(string path, string configuredRootDir)
    {
        var @namespace = $"CSnakes.Runtime";
        var pascalFileName = NamespaceHelper.AsDotNetClassName(path);
        var fileExtension = Path.GetExtension(path);
        var moduleAbsoluteName = Path.GetFileNameWithoutExtension(path).Replace('.', '_'); // Replace dots with underscores for module name
        if (!string.IsNullOrEmpty(configuredRootDir))
        {
            // Get path relative to the root directory
            var fileDirectory = Path.GetDirectoryName(path);

            // root directory can be a relative path with / denoting subfolders
            // Normalize this back into the platform-specific directory separator character
            var normalizedRootDir = configuredRootDir.Replace('/', Path.DirectorySeparatorChar);

            if (Path.GetFileName(fileDirectory) != normalizedRootDir)
            {
                var normalizedPath = GetPathAfter(path, normalizedRootDir) ?? throw new NamespaceNotInRootException(path, configuredRootDir);
                moduleAbsoluteName = NamespaceHelper.AsPythonImportPath(normalizedPath);
                var extraNamespace = NamespaceHelper.AsDotNetNamespace(normalizedPath);
                if (!string.IsNullOrEmpty(extraNamespace))
                {
                    @namespace += "." + extraNamespace;
                }
                pascalFileName = NamespaceHelper.AsDotNetClassName(normalizedPath);
            }
            // Otherwise there is a root directory, but the file is in the root directory itself, so we don't need to add anything to the namespace.
            // TODO: this condition could cause confusion if the configured root directory is "python" and the file is called "python/foo/python/bar.py"
        }
        else
        {
            // TODO: warn if the file is called __init__.py because there is no root directory
        }
        // Namespace the generated file names so they can't collide.
        var generatedFileName = $"{@namespace.RemoveNonLetterOrDigit()}.{pascalFileName}{fileExtension}.g.cs";

        return new DerivedNames
        {
            Namespace = @namespace,
            GeneratedFileName = generatedFileName,
            PascalFileName = pascalFileName,
            ModuleAbsoluteName = moduleAbsoluteName
        };
    }

    public static string FormatClassFromMethods(string @namespace, string pascalFileName, ImmutableArray<MethodDefinition> methods, string moduleAbsoluteName, PythonFunctionDefinition[] functions, SourceText sourceText, bool embedSourceText = false)
    {
        var functionNames = functions.Select(f => (Attr: f.Name, Field: $"__func_{f.Name}")).Distinct().ToImmutableArray();

#pragma warning disable format

        var sb = new StringBuilder();

        sb.AppendLine($$"""
            // <auto-generated/>
            #nullable enable

            #pragma warning disable PRTEXP001, PRTEXP002, CS0028

            using CSnakes.Runtime;
            using CSnakes.Runtime.Python;

            using System;
            using System.Collections.Generic;
            using System.Diagnostics;
            using System.Reflection.Metadata;
            using System.Text;
            using System.Threading;
            using System.Threading.Tasks;

            using Microsoft.Extensions.Logging;

            [assembly: MetadataUpdateHandler(typeof({{@namespace}}.{{pascalFileName}}Extensions))]

            namespace {{@namespace}};

            public static class {{pascalFileName}}Extensions
            {
                private static I{{pascalFileName}}? instance;

                private static ReadOnlySpan<byte> HotReloadHash => "{{HexString(sourceText.GetContentHash().AsSpan())}}"u8;

                public static I{{pascalFileName}} {{pascalFileName}}(this IPythonEnvironment env)
                {
                    if (instance is null)
                    {
                        instance = new {{pascalFileName}}Internal(env.Logger);
                    }
                    System.Diagnostics.Debug.Assert(!env.IsDisposed());
                    return instance;
                }

                public static void UpdateApplication(Type[]? updatedTypes)
                {
                    instance?.ReloadModule();
                }

                private class {{pascalFileName}}Internal : I{{pascalFileName}}
                {
                    private PyObject module;
                    private readonly ILogger<IPythonEnvironment>? logger;

            {{      Lines(IndentationLevel.Two,
                          from f in functionNames
                          select $"private PyObject {f.Field};") }}

                    internal {{pascalFileName}}Internal(ILogger<IPythonEnvironment>? logger)
                    {
                        this.logger = logger;
                        using (GIL.Acquire())
                        {
                            logger?.LogDebug("Importing module {ModuleName}", "{{moduleAbsoluteName}}");
                            this.module = ThisModule.Import();
            {{              Lines(IndentationLevel.Four,
                                  from f in functionNames
                                  select $"this.{f.Field} = module.GetAttr(\"{f.Attr}\");") }}
                        }
                    }

                    void IReloadableModuleImport.ReloadModule()
                    {
                        logger?.LogDebug("Reloading module {ModuleName}", "{{moduleAbsoluteName}}");
                        using (GIL.Acquire())
                        {
                            Import.ReloadModule(ref module);
                            // Dispose old functions
            {{              Lines(IndentationLevel.Four,
                                  from f in functionNames
                                  select $"this.{f.Field}.Dispose();") }}
                            // Bind to new functions
            {{              Lines(IndentationLevel.Four,
                                  from f in functionNames
                                  select $"this.{f.Field} = module.GetAttr(\"{f.Attr}\");") }}
                        }
                    }

                    public void Dispose()
                    {
                        logger?.LogDebug("Disposing module {ModuleName}", "{{moduleAbsoluteName}}");
            {{          Lines(IndentationLevel.Three,
                              from f in functionNames
                              select $"this.{f.Field}.Dispose();") }}
                        module.Dispose();
                    }

            {{      Lines(IndentationLevel.Two, methods.Select(m => m.Syntax).Compile().TrimEnd()) }}
                }
            }

            /// <summary>
            /// Represents functions of the Python module <c>{{moduleAbsoluteName}}</c>.
            /// </summary>
            public interface I{{pascalFileName}} : IReloadableModuleImport
            {
            {{  Lines(IndentationLevel.One,
                      Enumerable.Skip(count: 1, source:
                          from m in methods.Select(m => new
                          {
                              m.Syntax,
                              FunctionName = m.PythonFunction.Name,
                              m.PythonFunction.SourceLines,
                          })
                          let s = m.Syntax.Identifier.Text == "ReloadModule"
                                 // This prevents the warning:
                                 // > warning CS0108: 'IFooBar.ReloadModule()' hides inherited member 'IReloadableModuleImport.ReloadModule()'. Use the new keyword if hiding was intended.
                                 // because `IReloadableModuleImport` already has a `ReloadModule` method.
                                 ? m.Syntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.NewKeyword))
                                 : m.Syntax
                          from lines in new string[][]
                          {

                              [
                                  "",
                                  "/// <summary>",
                                  $"/// Invokes the Python function <c>{m.FunctionName}</c>:",
                                  "/// <code><![CDATA[",
                                  ..from line in m.SourceLines
                                    select line.ToString() into line
                                    select $"/// {line}{(line.EndsWith(":") ? " ..." : null)}",
                                  "/// ]]></code>",
                                  "/// </summary>"
                              ],
                              [
                                  $"{s.WithModifiers(new SyntaxTokenList(s.Modifiers.Where(m => !m.IsKind(SyntaxKind.PublicKeyword) && !m.IsKind(SyntaxKind.AsyncKeyword)).ToList()))
                                      .WithBody(null)
                                      .NormalizeWhitespace()};"
                              ]
                          }
                          from line in lines
                          select line)) }}
            }

            """);

        if (embedSourceText)
        {
            // Note the use of quintuple-quoted raw string literal here so that the generated
            // "source" field below can be quadruple-quoted, and which in turn allows safe embedding
            // of Python source code that could potentially contain triple-quoted strings.

            sb.AppendLine($$"""""
                file static class ThisModule
                {
                    private static ReadOnlySpan<byte> source => """"
                {{      Lines(IndentationLevel.Two, sourceText)}}
                        """"u8;

                    public static PyObject Import() =>
                        CSnakes.Runtime.Python.Import.ImportModule("{{moduleAbsoluteName}}", source, "{{moduleAbsoluteName}}.py");
                }
                """"");
        }
        else
        {
            sb.AppendLine($$"""
                file static class ThisModule
                {
                    public static PyObject Import() =>
                        CSnakes.Runtime.Python.Import.ImportModule("{{moduleAbsoluteName}}");
                }
                """);
        }

#pragma warning restore format

        return sb.ToString();
    }

    private static string HexString(ReadOnlySpan<byte> bytes)
    {
        const string hexChars = "0123456789abcdef";

        var chars = new char[bytes.Length * 2];
        var i = 0;
        foreach (var b in bytes)
        {
            chars[i++] = hexChars[b >> 4];
            chars[i++] = hexChars[b & 0xF];
        }

        return new string(chars);
    }

    const string Space = " ";
    const string Indent = $"{Space}{Space}{Space}{Space}";

    private static readonly string[] Indents =
    [
        "",
        Indent,
        Indent + Indent,
        Indent + Indent + Indent,
        Indent + Indent + Indent + Indent,
    ];

    private enum IndentationLevel { Zero = 0, One = 1, Two = 2, Three = 3, Four = 4 }

    private static FormattableLines Lines(IndentationLevel level, string lines) =>
        Lines(level, SourceText.From(lines));

    private static FormattableLines Lines(IndentationLevel level, SourceText sourceText) =>
        Lines(level, from line in sourceText.Lines
                     select line.ToString());

    private static FormattableLines Lines(IndentationLevel level, IEnumerable<string> lines) =>
        new([.. lines], Indents[(int)level], string.Empty);

    private sealed class FormattableLines(ImmutableArray<string> lines,
                                          string? prefix = null,
                                          string? emptyPrefix = null) :
        IFormattable
    {
        string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
        {
            if (lines.Length == 0)
                return string.Empty;

            var writer = new StringWriter();
            var lastIndex = lines.Length - 1;
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];

                if (line.Length > 0)
                {
                    if (prefix is { } somePrefix)
                        writer.Write(somePrefix);
                }
                else if (emptyPrefix is { } someEmptyPrefix)
                {
                    writer.Write(someEmptyPrefix);
                }

                if (i < lastIndex)
                    writer.WriteLine(line);
                else
                    writer.Write(line);
            }

            return writer.ToString();
        }

        public override string ToString()
        {
            IFormattable formattable = this;
            return formattable.ToString(format: null, formatProvider: null);
        }
    }
}
