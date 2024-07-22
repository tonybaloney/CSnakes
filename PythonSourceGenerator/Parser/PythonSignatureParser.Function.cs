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

    public static bool TryParseFunctionDefinitions(string source, out PythonFunctionDefinition[] pythonSignatures, out GeneratorError[] errors)
    {
        List<PythonFunctionDefinition> functionDefinitions = [];

        // Go line by line
        var lines = source.Split(["\r\n", "\n"], StringSplitOptions.None);
        var currentErrors = new List<GeneratorError>();
        List<(int startLine, int endLine, string code)> functionLines = [];
        List<string> currentBuffer = [];
        int currentBufferStartLine = -1;
        bool unfinishedFunctionSpec = false;
        for (int i = 0; i < lines.Length; i++)
        {
            if (IsFunctionSignature(lines[i]) || unfinishedFunctionSpec)
            {
                currentBuffer.Add(lines[i]);
                if (currentBufferStartLine == -1)
                {
                    currentBufferStartLine = i;
                }
                // Parse the function signature
                var result = PythonSignatureTokenizer.Instance.TryTokenize(lines[i]);
                if (!result.HasValue)
                {
                    // TODO: Work out end column and add to the other places in this function where it's raised
                    currentErrors.Add(new GeneratorError(i, i, result.ErrorPosition.Column, result.ErrorPosition.Column, result.FormatErrorMessageFragment()));

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
                    functionLines.Add((currentBufferStartLine, i, string.Join("", currentBuffer)));
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
        }
        foreach (var line in functionLines)
        {
            // TODO: (track) This means we end up tokenizing the lines twice (one individually and again merged). Optimize.
            var result = PythonSignatureTokenizer.Instance.TryTokenize(line.code);
            if (!result.HasValue)
            {
                currentErrors.Add(new GeneratorError(line.startLine, line.endLine, result.ErrorPosition.Column, result.ErrorPosition.Column, result.FormatErrorMessageFragment()));
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
                currentErrors.Add(new GeneratorError(line.startLine, line.endLine, functionDefinition.ErrorPosition.Column, functionDefinition.ErrorPosition.Column + 1, functionDefinition.FormatErrorMessageFragment()));
            }
        }

        pythonSignatures = [.. functionDefinitions];
        errors = [.. currentErrors];
        return errors.Length == 0;
    }
}
