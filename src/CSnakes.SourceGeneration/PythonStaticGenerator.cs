using CSnakes.Parser;
using CSnakes.Parser.Types;
using CSnakes.Reflection;
using CSnakes.SourceGeneration.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace CSnakes;

[Generator(LanguageNames.CSharp)]
public class PythonStaticGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // System.Diagnostics.Debugger.Launch();
        var pythonFilesPipeline = context.AdditionalTextsProvider
            .Where(static text => Path.GetExtension(text.Path) == ".py");

        context.RegisterSourceOutput(pythonFilesPipeline, static (sourceContext, file) =>
        {
            // Add environment path
            var @namespace = "CSnakes.Runtime";

            var fileName = Path.GetFileNameWithoutExtension(file.Path);

            // Convert snake_case to PascalCase
            var pascalFileName = string.Join("", fileName.Split('_').Select(s => char.ToUpperInvariant(s[0]) + s[1..]));
            // Read the file
            var code = file.GetText(sourceContext.CancellationToken);

            if (code is null) return;

            // Calculate hash of code
            var hash = code.GetContentHash();

            // Parse the Python file
            var result = PythonParser.TryParseFunctionDefinitions(code, out PythonFunctionDefinition[] functions, out GeneratorError[]? errors);

            foreach (var error in errors)
            {
                // Update text span
                Location errorLocation = Location.Create(file.Path, TextSpan.FromBounds(0, 1), new LinePositionSpan(new LinePosition(error.StartLine, error.StartColumn), new LinePosition(error.EndLine, error.EndColumn)));
                sourceContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("PSG004", "PythonStaticGenerator", error.Message, "PythonStaticGenerator", DiagnosticSeverity.Error, true), errorLocation));
            }

            if (result)
            {
                var methods = ModuleReflection.MethodsFromFunctionDefinitions(functions, fileName).ToImmutableArray();
                string source = FormatClassFromMethods(@namespace, pascalFileName, methods, fileName, functions, hash);
                sourceContext.AddSource($"{pascalFileName}.py.cs", source);
                sourceContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("PSG002", "PythonStaticGenerator", $"Generated {pascalFileName}.py.cs", "PythonStaticGenerator", DiagnosticSeverity.Info, true), Location.None));
            }
        });

        var methodReflectors = context.SyntaxProvider
                          .ForAttributeWithMetadataName("CSnakes.Runtime.Reflection.PythonMethodAttribute",
                                                        MethodAttributePartials.CouldBeMethod,
                                                        MethodAttributePartials.GetMethodInfo)
                          .Collect()
                          .SelectMany((enumInfos, _) => enumInfos.Distinct());

        context.RegisterSourceOutput(methodReflectors, static (context, model) =>
        {
            context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("PSG010", "PythonStaticGenerator", "Reflecting Method", "PythonStaticGenerator", DiagnosticSeverity.Info, true), Location.None));
            context.AddSource(model.GeneratedFileName, SourceText.From(model.SourceText, Encoding.UTF8));
        });
    }

    

    public static string FormatClassFromMethods(string @namespace, string pascalFileName, ImmutableArray<MethodDefinition> methods, string fileName, PythonFunctionDefinition[] functions, ImmutableArray<byte> hash)
    {
        var paramGenericArgs = methods
            .Select(m => m.ParameterGenericArgs)
            .Where(l => l is not null && l.Any());

        var functionNames = functions.Select(f => (Attr: f.Name, Field: $"__func_{f.Name}")).ToImmutableArray();

#pragma warning disable format

        return $$"""
            // <auto-generated/>
            #nullable enable

            using CSnakes.Runtime;
            using CSnakes.Runtime.Python;

            using System;
            using System.Collections.Generic;
            using System.Diagnostics;
            using System.Reflection.Metadata;

            using Microsoft.Extensions.Logging;

            [assembly: MetadataUpdateHandler(typeof({{@namespace}}.{{pascalFileName}}Extensions))]

            namespace {{@namespace}};

            public static class {{pascalFileName}}Extensions
            {
                private static I{{pascalFileName}}? instance;

                private static ReadOnlySpan<byte> HotReloadHash => "{{HexString(hash.AsSpan())}}"u8;

                public static I{{pascalFileName}} {{pascalFileName}}(this IPythonEnvironment env)
                {
                    if (instance is null)
                    {
                        instance = new {{pascalFileName}}Internal(env.Logger);
                    }
                    Debug.Assert(!env.IsDisposed());
                    return instance;
                }

                public static void UpdateApplication(Type[]? updatedTypes)
                {
                    instance?.ReloadModule();
                }

                private class {{pascalFileName}}Internal : I{{pascalFileName}}
                {
                    private PyObject module;
                    private readonly ILogger<IPythonEnvironment> logger;

            {{      Lines(IndentationLevel.Two,
                          from f in functionNames
                          select $"private PyObject {f.Field};") }}

                    internal {{pascalFileName}}Internal(ILogger<IPythonEnvironment> logger)
                    {
                        this.logger = logger;
                        using (GIL.Acquire())
                        {
                            logger.LogDebug("Importing module {ModuleName}", "{{fileName}}");
                            module = Import.ImportModule("{{fileName}}");
            {{              Lines(IndentationLevel.Four,
                                  from f in functionNames
                                  select $"this.{f.Field} = module.GetAttr(\"{f.Attr}\");") }}
                        }
                    }

                    void IReloadableModuleImport.ReloadModule()
                    {
                        logger.LogDebug("Reloading module {ModuleName}", "{{fileName}}");
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
                        logger.LogDebug("Disposing module {ModuleName}", "{{fileName}}");
            {{          Lines(IndentationLevel.Three,
                              from f in functionNames
                              select $"this.{f.Field}.Dispose();") }}
                        module.Dispose();
                    }

            {{      Lines(IndentationLevel.Two, methods.Select(m => m.Syntax).Compile().TrimEnd()) }}
                }
            }

            /// <summary>
            /// Represents functions of the Python module <c>{{fileName}}</c>.
            /// </summary>
            public interface I{{pascalFileName}} : IReloadableModuleImport
            {
            {{  Lines(IndentationLevel.One,
                      Enumerable.Skip(count: 1, source:
                          from m in methods.Zip(functions, (m, f) => new
                          {
                              m.Syntax,
                              FunctionName = f.Name,
                              f.SourceLines,
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

            """;
#pragma warning restore format
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
        Lines(level, from line in SourceText.From(lines).Lines
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
