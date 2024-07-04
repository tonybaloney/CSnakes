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

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonFunctionParameter> PythonParameter { get; } = 
        from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
        from colon in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Colon)
        from type in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
        select new PythonFunctionParameter { Name = name.ToStringValue(), Type = type.ToStringValue() };

    // Python parameter list
    //static TokenListParser<PythonSignatureTokens.PythonSignatureToken, object[]> PythonParameterList { get; } = 
    //    from openParen in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.OpenParenthesis)
    //    from parameters in PythonParameter.ManyDelimitedBy(Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Comma))
    //    from closeParen in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.CloseParenthesis)
    //    select parameters;

    //static TokenListParser<PythonSignatureTokens.PythonSignatureToken, object?> PythonFunctionDefinition { get; } = 
    //    from def in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Def)
    //    from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
    //    from parameters in PythonParameterList
    //    from arrow in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Arrow)
    //    from returnType in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
    //    from colon in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Colon)
    //    select new { Name = name, Parameters = parameters, ReturnType = returnType };

    // static TokenListParser<PythonSignatureTokens.PythonSignatureToken, object?> PythonStatement { get; } = 

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
