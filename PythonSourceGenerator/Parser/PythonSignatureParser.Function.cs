using Microsoft.CodeAnalysis.Text;
using PythonSourceGenerator.Parser.Types;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

using ParsedTokens = Superpower.Model.TokenList<PythonSourceGenerator.Parser.PythonSignatureTokens.PythonSignatureToken>;

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
        // Go line by line
        TextLineCollection lines = source.Lines;
        List<GeneratorError> currentErrors = [];
        List<(IEnumerable<TextLine> lines, ParsedTokens tokens)> functionLines = [];
        List<(TextLine line, ParsedTokens tokens)> currentBuffer = [];
        bool unfinishedFunctionSpec = false;
        foreach (TextLine line in lines)
        {
            string lineOfCode = line.ToString();
            if (!IsFunctionSignature(lineOfCode) && !unfinishedFunctionSpec)
            {
                continue;
            }

            // Parse the function signature
            Result<ParsedTokens> result = PythonSignatureTokenizer.Instance.TryTokenize(lineOfCode);
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
                IEnumerable<TextLine> bufferLines = currentBuffer.Select(x => x.line);
                ParsedTokens tokens = new(currentBuffer.SelectMany(x => x.tokens).ToArray());

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

        List<PythonFunctionDefinition> functionDefinitions = [];

        foreach ((IEnumerable<TextLine> currentLines, ParsedTokens tokens) in functionLines)
        {
            TokenListParserResult<PythonSignatureTokens.PythonSignatureToken, PythonFunctionDefinition> functionDefinition =
                PythonFunctionDefinitionTokenizer.TryParse(tokens);
            if (functionDefinition.HasValue)
            {
                functionDefinitions.Add(functionDefinition.Value);
            }
            else
            {
                // Error parsing the function definition
                int lineNumber = currentLines.First().LineNumber;
                currentErrors.Add(new(
                    lineNumber,
                    lineNumber + functionDefinition.ErrorPosition.Line - 1,
                    functionDefinition.ErrorPosition.Column,
                    functionDefinition.ErrorPosition.Column + 1,
                    functionDefinition.FormatErrorMessageFragment())
                );
            }
        }

        pythonSignatures = [.. functionDefinitions];
        errors = [.. currentErrors];
        return errors.Length == 0;
    }
}
