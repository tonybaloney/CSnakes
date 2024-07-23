using Microsoft.CodeAnalysis.Text;
using PythonSourceGenerator.Parser.Types;
using Superpower;
using Superpower.Parsers;

namespace PythonSourceGenerator.Parser;
public static partial class PythonSignatureParser
{
    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonFunctionDefinition> PythonFunctionDefinitionTokenizer { get; } =
        (from def in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Def)
         from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
         from parameters in PythonParameterListTokenizer
         from arrow in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Arrow).Optional().Then(returnType => PythonTypeDefinitionTokenizer.OptionalOrDefault())
         from colon in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Colon)
         select new PythonFunctionDefinition(name.ToStringValue(), arrow, parameters))
        .Named("Function Definition");

    /// <summary>
    /// Checks if the line starts with def.
    /// </summary>
    /// <param name="line">Line to check</param>
    /// <returns></returns>
    static bool IsFunctionSignature(string line) =>
        line.StartsWith("def ") || line.StartsWith("async def");

    public static bool TryParseFunctionDefinitions(SourceText source, out PythonFunctionDefinition[] pythonSignatures, out GeneratorError[] errors)
    {
        List<PythonFunctionDefinition> functionDefinitions = [];

        // Go line by line
        TextLineCollection lines = source.Lines;
        List<GeneratorError> currentErrors = [];
        List<(int startLine, int endLine, string code)> functionLines = [];
        List<string> currentBuffer = [];
        int currentBufferStartLine = -1;
        bool unfinishedFunctionSpec = false;
        foreach (TextLine line in lines)
        {
            string lineOfCode = line.ToString();
            if (!IsFunctionSignature(lineOfCode) && !unfinishedFunctionSpec)
            {
                continue;
            }

            currentBuffer.Add(lineOfCode);
            if (currentBufferStartLine == -1)
            {
                currentBufferStartLine = line.LineNumber;
            }
            // Parse the function signature
            var result = PythonSignatureTokenizer.Instance.TryTokenize(lineOfCode);
            if (!result.HasValue)
            {
                currentErrors.Add(new GeneratorError(line.LineNumber, line.LineNumber, result.ErrorPosition.Column, line.End, result.FormatErrorMessageFragment()));

                // Reset buffer
                currentBuffer = [];
                currentBufferStartLine = -1;
                unfinishedFunctionSpec = false;
                continue;
            }

            // If this is a function definition on one line..
            if (result.Value.Last().Kind == PythonSignatureTokens.PythonSignatureToken.Colon)
            {
                // TODO: (track) Is an empty string the right joining character?
                functionLines.Add((currentBufferStartLine, line.LineNumber, string.Join("", currentBuffer)));
                currentBuffer = [];
                currentBufferStartLine = -1;
                unfinishedFunctionSpec = false;
                continue;
            }
            else
            {
                unfinishedFunctionSpec = true;
            }
        }
        foreach ((int startLine, int endLine, string code) in functionLines)
        {
            // TODO: (track) This means we end up tokenizing the lines twice (one individually and again merged). Optimize.
            var result = PythonSignatureTokenizer.Instance.TryTokenize(code);
            if (!result.HasValue)
            {
                currentErrors.Add(new GeneratorError(startLine, endLine, result.ErrorPosition.Column, result.ErrorPosition.Column, result.FormatErrorMessageFragment()));
                continue;
            }
            var functionDefinition = PythonFunctionDefinitionTokenizer.TryParse(result.Value);
            if (functionDefinition.HasValue)
            {
                functionDefinitions.Add(functionDefinition.Value);
            }
            else
            {
                // Error parsing the function definition
                currentErrors.Add(new GeneratorError(startLine, endLine, functionDefinition.ErrorPosition.Column, functionDefinition.ErrorPosition.Column + 1, functionDefinition.FormatErrorMessageFragment()));
            }
        }

        pythonSignatures = [.. functionDefinitions];
        errors = [.. currentErrors];
        return errors.Length == 0;
    }
}
