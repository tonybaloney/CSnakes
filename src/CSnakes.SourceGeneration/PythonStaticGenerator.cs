using CSnakes.Parser;
using CSnakes.Parser.Types;
using CSnakes.Reflection;
using CSnakes.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace CSnakes;

[Generator(LanguageNames.CSharp)]
public class PythonStaticGenerator : IIncrementalGenerator
{
    private sealed class KnownTypeSymbols
    {
        private CacheEntry readOnlyList;
        private CacheEntry readOnlyDictionary;
        private WeakReference<Compilation>? compilation;

        public bool IsReadOnlyList(ITypeSymbol type, Compilation compilation) =>
            Is(type, ref this.readOnlyList, compilation, "System.Collections.Generic.IReadOnlyList`1");

        public bool IsReadOnlyDictionary(ITypeSymbol type, Compilation compilation) =>
            Is(type, ref this.readOnlyDictionary, compilation, "System.Collections.Generic.IReadOnlyDictionary`2");

        private bool Is(ITypeSymbol type, ref CacheEntry entry, Compilation compilation, string name)
        {
            if (this.compilation is null
                || !this.compilation.TryGetTarget(out var cacheCompilation)
                || cacheCompilation != compilation)
            {
                Reset(compilation);
            }

            if (!entry.Resolved)
            {
                entry.NamedTypeSymbol = compilation.GetTypeByMetadataName(name);
                entry.Resolved = true;
            }

            return SymbolEqualityComparer.Default.Equals(type, entry.NamedTypeSymbol);
        }

        private void Reset(Compilation compilation)
        {
            this.readOnlyList = new();
            this.readOnlyDictionary = new();
            this.compilation = new WeakReference<Compilation>(compilation);
        }

        private struct CacheEntry
        {
            public bool Resolved;
            public INamedTypeSymbol? NamedTypeSymbol;
        }
    }

    private static TypeInfo? Map(Compilation compilation, ITypeSymbol type, KnownTypeSymbols typeSymbols)
    {
        return type switch
        {
            { SpecialType: SpecialType.System_Boolean } => TypeInfo.Boolean,
            { SpecialType: SpecialType.System_String } => TypeInfo.String,
            { SpecialType: SpecialType.System_Int64 } => TypeInfo.Int64,
            { SpecialType: SpecialType.System_Double } => TypeInfo.Double,
            IArrayTypeSymbol { Rank: 1, ElementType.SpecialType: SpecialType.System_Byte } => TypeInfo.ByteArray,
            INamedTypeSymbol { IsGenericType: true, TypeArguments: [var it], OriginalDefinition: var td }
                when typeSymbols.IsReadOnlyList(td, compilation)
                  && Map(compilation, it, typeSymbols) is { } iti =>
                new ListTypeInfo(iti),
            INamedTypeSymbol { IsGenericType: true, TypeArguments: [var kt, var vt], OriginalDefinition: var td }
                when typeSymbols.IsReadOnlyDictionary(td, compilation)
                  && Map(compilation, kt, typeSymbols) is { } kti
                  && Map(compilation, vt, typeSymbols) is { } vti =>
                new DictionaryTypeInfo(kti, vti),
            _ => null
        };
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // System.Diagnostics.Debugger.Launch();
        var pythonFilesPipeline = context.AdditionalTextsProvider
            .Where(static text => Path.GetExtension(text.Path) == ".py");

        var knownSymbols = new KnownTypeSymbols();

        var conversions =
            context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is InvocationExpressionSyntax
                {
                    Expression: MemberAccessExpressionSyntax
                    {
                        Name: GenericNameSyntax { Identifier.Text: "As", Arity: 1 }
                    },
                    ArgumentList.Arguments.Count: 0
                },
                transform: (context, cancellationToken) =>
                {
                    if (context.SemanticModel.GetOperation(context.Node) is not IInvocationOperation
                        {
                            Syntax: InvocationExpressionSyntax
                            {
                                Expression: MemberAccessExpressionSyntax
                                {
                                    Name: var methodName
                                }
                            } invocation,
                            TargetMethod:
                            {
                                Kind: SymbolKind.Method,
                                IsStatic: false,
                                Name: "As",
                                IsGenericMethod: true,
                                TypeArguments: [var typeArg],
                                Parameters.Length: 0,
                                ContainingType:
                                {
                                    Name: "PyObject",
                                    ContainingNamespace:
                                    {
                                        Name: "Python",
                                        ContainingNamespace:
                                        {
                                            Name: "Runtime",
                                            ContainingNamespace:
                                            {
                                                Name: "CSnakes",
                                                ContainingNamespace.IsGlobalNamespace: true
                                            }
                                        }
                                    }
                                }
                            }
                        })
                    {
                        return null;
                    }

                    if (Map(context.SemanticModel.Compilation, typeArg, knownSymbols) is not { } typeInfo)
                        return null;

#pragma warning disable RSEXPERIMENTAL002
                    if (context.SemanticModel.GetInterceptableLocation(invocation, cancellationToken) is not { } interceptableLocation
#pragma warning restore RSEXPERIMENTAL002
                        || SourceCodeLocation.CreateFrom(methodName.GetLocation()) is not { } sourceCodeLocation)
                    {
                        return null;
                    }

                    return new
                    {
                        Index = -1,
                        TypeInfo = typeInfo,
                        InterceptableLocation = interceptableLocation,
                        SourceCodeLocation = sourceCodeLocation,
                    };
                });

        var options =
            context.AnalyzerConfigOptionsProvider
                   .Select(static (context, _) =>
                       context.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out var projectDirPath)
                           ? projectDirPath
                           : null);

        context.RegisterSourceOutput(
            conversions.Where(e => e is not null)
                       .Collect()
                       .Combine(options),
            static (context, combo) =>
            {
                var (conversions, options) = combo;

                var interceptors =
                    from gs in new[]
                    {
                        from e in conversions
                        group e by e.TypeInfo
                    }
                    from g in gs.Select(static (g, i) => new { Ordinal = i + 1, Type = g.Key, Members = g })
                    let header =
                        string.Join(Environment.NewLine,
                            from m in g.Members
#pragma warning disable RSEXPERIMENTAL002
                            from line in new[]
                            {
                                options is { } projectDirPath
                                    ? $"// {new Uri($"{projectDirPath}/").MakeRelativeUri(new(m.SourceCodeLocation.FilePath))}:{m.SourceCodeLocation.LineSpan}"
                                    : null,
                                m.InterceptableLocation.GetInterceptsLocationAttributeSyntax(),
                            }
                            where line is not null
                            select $"        {line}")
                    let type = new
                    {
                        Return = g.Type.GetReturnTypeSyntax(),
                        Importer = g.Type.GetImporterTypeSyntax(),
                    }
                    select $"""
                            {header}
                                    public static {type.Return} As{g.Ordinal}(this global::CSnakes.Runtime.Python.PyObject obj) =>
                                        obj.ImportAs<{type.Return}, {type.Importer}>();
                            """;
#pragma warning restore RSEXPERIMENTAL002

                var source = $$"""
                    //------------------------------------------------------------------------------
                    // <auto-generated>
                    //     This code was generated by a tool.
                    //
                    //     Changes to this file may cause incorrect behavior and will be lost if
                    //     the code is regenerated.
                    // </auto-generated>
                    //------------------------------------------------------------------------------

                    #nullable enable

                    namespace System.Runtime.CompilerServices
                    {
                        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
                        file sealed class InterceptsLocationAttribute : global::System.Attribute
                        {
                            public InterceptsLocationAttribute(int version, string data) { }
                        }
                    }

                    #pragma warning disable PRTEXP001

                    namespace CSnakes.Runtime.Python.Generated
                    {
                        file static class Interceptions
                        {
                    {{string.Join($"{Environment.NewLine}{Environment.NewLine}", interceptors)}}
                        }
                    }

                    """;

                context.AddSource("PyObjectAsInterceptions.g.cs", source);
            });

        context.RegisterSourceOutput(pythonFilesPipeline, static (sourceContext, file) =>
        {
            // Add environment path
            var @namespace = "CSnakes.Runtime";

            var fileName = Path.GetFileNameWithoutExtension(file.Path);

            // Convert snake_case to PascalCase
            var pascalFileName = string.Join("", fileName.Split('_').Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1)));
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

            #pragma warning disable PRTEXP001, PRTEXP002

            using CSnakes.Runtime;
            using CSnakes.Runtime.Python;

            using System;
            using System.Collections.Generic;
            using System.Diagnostics;
            using System.Reflection.Metadata;
            using System.Threading.Tasks;

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
