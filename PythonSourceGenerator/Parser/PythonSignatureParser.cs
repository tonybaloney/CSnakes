using PythonSourceGenerator.Parser.Types;
using Superpower;
using Superpower.Parsers;
using System;
using System.Collections.Generic;

namespace PythonSourceGenerator.Parser;
public static class PythonSignatureParser
{
    public static bool IsFunctionSignature(string line)
    {
        // Check if the line starts with "def"
        return line.StartsWith("def ") || line.StartsWith("async def");
    }

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonTypeSpec> PythonTypeDefinitionTokenizer { get; } =
        from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
        from openBracket in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.OpenBracket).Then(_ => PythonTypeDefinitionTokenizer.ManyDelimitedBy(Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Comma))).OptionalOrDefault()
        from closeBracket in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.CloseBracket).Optional()
        select new PythonTypeSpec { Name = name.ToStringValue(), Arguments = openBracket };

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonFunctionParameter> PythonParameterTokenizer { get; } = 
        from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
        from colon in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Colon).Optional()
        from type in PythonTypeDefinitionTokenizer.OptionalOrDefault()
        select new PythonFunctionParameter { Name = name.ToStringValue(), Type = type };

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonFunctionParameter[]> PythonParameterListTokenizer { get; } = 
        from openParen in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.OpenParenthesis)
        from parameters in PythonParameterTokenizer.ManyDelimitedBy(Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Comma))
        from closeParen in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.CloseParenthesis)
        select parameters;

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonFunctionDefinition> PythonFunctionDefinitionTokenizer { get; } =
        from def in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Def)
        from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
        from parameters in PythonParameterListTokenizer
        from arrow in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Arrow).Optional().Then(returnType => PythonTypeDefinitionTokenizer.OptionalOrDefault())
        from colon in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Colon)
        select new PythonFunctionDefinition { Name = name.ToStringValue(), Parameters = parameters, ReturnType = arrow };

    public static bool TryParseFunctionDefinitions(string source, out PythonFunctionDefinition[]? pythonSignatures)
    {
        List<PythonFunctionDefinition> functionDefinitions = [];

        // Go line by line
        var lines = source.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            if (IsFunctionSignature(line))
            {
                // Parse the function signature
                var lineTokens = PythonSignatureTokenizer.Tokenize(line);

                // TODO : Functions that span multiple lines

                var functionDefinition = PythonFunctionDefinitionTokenizer.TryParse(lineTokens);
                if (functionDefinition.HasValue) {
                    functionDefinitions.Add(functionDefinition.Value);
                }
                else
                {
                    // Error parsing the function definition
                    pythonSignatures = null;
                    // TODO : Add reason
                    return false;
                }
            }
        }
        pythonSignatures = [.. functionDefinitions];
        return true;
    }
}
