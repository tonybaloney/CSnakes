using Microsoft.CodeAnalysis.Text;
using PythonSourceGenerator.Parser.Types;
using Superpower;
using Superpower.Model;
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
        List<(IEnumerable<TextLine> lines, TokenList<PythonSignatureTokens.PythonSignatureToken> tokens)> functionLines = [];
        List<(TextLine line, TokenList<PythonSignatureTokens.PythonSignatureToken> tokens)> currentBuffer = [];
        bool unfinishedFunctionSpec = false;
        foreach (TextLine line in lines)
        {
            string lineOfCode = line.ToString();
            if (!IsFunctionSignature(lineOfCode) && !unfinishedFunctionSpec)
            {
                continue;
            }

            // Parse the function signature
            var result = PythonSignatureTokenizer.Instance.TryTokenize(lineOfCode);
            if (!result.HasValue)
            {
                currentErrors.Add(new GeneratorError(line.LineNumber, line.LineNumber, result.ErrorPosition.Column, line.End, result.FormatErrorMessageFragment()));

                // Reset buffer
                currentBuffer = [];
                unfinishedFunctionSpec = false;
                continue;
            }
            currentBuffer.Add((line, result.Value));

            // If this is a function definition on one line..
            if (result.Value.Last().Kind == PythonSignatureTokens.PythonSignatureToken.Colon)
            {
                var bufferLines = currentBuffer.Select(x => x.line);
                var tokens = new TokenList<PythonSignatureTokens.PythonSignatureToken>(currentBuffer.SelectMany(x => x.tokens).ToArray());

                functionLines.Add((bufferLines, tokens));
                currentBuffer = [];
                unfinishedFunctionSpec = false;
                continue;
            }
            else
            {
                unfinishedFunctionSpec = true;
            }
        }
        foreach (var (currentLines, tokens) in functionLines)
        {
            var functionDefinition = PythonFunctionDefinitionTokenizer.TryParse(tokens);
            if (functionDefinition.HasValue)
            {
                functionDefinitions.Add(functionDefinition.Value);
            }
            else
            {
                // Error parsing the function definition
                currentErrors.Add(new GeneratorError(currentLines.First().LineNumber, currentLines.First().LineNumber + functionDefinition.ErrorPosition.Line - 1, functionDefinition.ErrorPosition.Column, functionDefinition.ErrorPosition.Column + 1, functionDefinition.FormatErrorMessageFragment()));
            }
        }

        pythonSignatures = [.. functionDefinitions];
        errors = [.. currentErrors];
        return errors.Length == 0;
    }
}
