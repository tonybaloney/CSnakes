using PythonSourceGenerator.Parser.Types;
using Superpower;
using Superpower.Parsers;
using System;

namespace PythonSourceGenerator.Parser;
public static class PythonSignatureParser
{
    public static bool IsFunctionSignature(string line)
    {
        // Check if the line starts with "def"
        return line.StartsWith("def ") || line.StartsWith("async def");
    }

    // Should match list, list[T], tuple[int, str], dict[str, int]
    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonTypeSpec> PythonTypeDefinition { get; } =
        from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
        from openBracket in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.OpenBracket).Then(_ => PythonTypeDefinition.ManyDelimitedBy(Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Comma))).OptionalOrDefault()
        from closeBracket in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.CloseBracket).Optional()
        select new PythonTypeSpec { Name = name.ToStringValue(), Arguments = openBracket };

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonFunctionParameter> PythonParameter { get; } = 
        from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
        from colon in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Colon).Optional()
        from type in PythonTypeDefinition.OptionalOrDefault()
        select new PythonFunctionParameter { Name = name.ToStringValue(), Type = type };

    // Python parameter list
    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonFunctionParameter[]> PythonParameterList { get; } = 
        from openParen in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.OpenParenthesis)
        from parameters in PythonParameter.ManyDelimitedBy(Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Comma))
        from closeParen in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.CloseParenthesis)
        select parameters;

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonFunctionDefinition> PythonFunctionDefinition { get; } =
        from def in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Def)
        from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
        from parameters in PythonParameterList
        from arrow in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Arrow).Optional().Then(returnType => PythonTypeDefinition.OptionalOrDefault())
        from colon in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Colon)
        select new PythonFunctionDefinition { Name = name.ToStringValue(), Parameters = parameters, ReturnType = arrow };

    public static bool TryParse(string signature, out object? pythonSignature)
    {
        pythonSignature = null;

        // Go line by line
        var lines = signature.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            if (IsFunctionSignature(line))
            {
                // Parse the function signature
                var lineTokens = PythonSignatureTokenizer.Instance.TryTokenize(line);

                if (!lineTokens.HasValue)
                {
                    break;
                }

                // Is the last token a colon?
                //if (lineTokens.ConsumeToken().Value != PythonSignatureTokens.PythonSignatureToken.Colon)
                //{
                //    pythonSignature = null;
                //    return false;
                //}
            }
        }
        return false;
    }
}
