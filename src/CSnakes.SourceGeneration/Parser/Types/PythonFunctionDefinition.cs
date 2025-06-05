using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace CSnakes.Parser.Types;
public class PythonFunctionDefinition(string name, PythonTypeSpec? returnType, PythonFunctionParameterList parameters, bool isAsync = false)
{
    public string Name { get; private set; } = name;

    public PythonTypeSpec ReturnType => returnType ?? PythonTypeSpec.Any;

    public PythonFunctionParameterList Parameters => parameters;

    public bool HasReturnTypeAnnotation() => returnType is not null;

    public bool IsAsync => isAsync;

    public ImmutableArray<TextLine> SourceLines { get; private set; } = [];

    public PythonFunctionDefinition WithSourceLines(ImmutableArray<TextLine> value) =>
        value == SourceLines ? this : new(Name, ReturnType, Parameters, IsAsync)
        {
            SourceLines = value
        };
}
