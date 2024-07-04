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

    public static TextParser<int> IntegerConstant { get; } =
        from sign in Character.EqualTo('-').OptionalOrDefault()
        from digits in Character.Digit.Many()
        select Convert.ToInt32(digits.ToString());

    public static TextParser<double> FloatConstant { get; } =
        from sign in Character.EqualTo('-').OptionalOrDefault()
        from digits in Character.Digit.Many()
        from decimal_ in Character.EqualTo('.')
        from rest in Character.Digit.Many()
        select Convert.ToDouble(digits.ToString() + '.' + rest.ToString());

    public static TextParser<string> DoubleQuotedStringConstant { get; } =
        from open in Character.EqualTo('"')
        from chars in Character.ExceptIn('"').Many()
        from close in Character.EqualTo('"')
        select new string(chars);

    public static TextParser<string> SingleQuotedStringConstant { get; } =
        from open in Character.EqualTo('\'')
        from chars in Character.ExceptIn('\'').Many()
        from close in Character.EqualTo('\'')
        select new string(chars);

    public static TextParser<string> StringConstant { get; } =
        SingleQuotedStringConstant.AsNullable()
        .Or(DoubleQuotedStringConstant.AsNullable())
        .Named("String Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonTypeSpec> PythonTypeDefinitionTokenizer { get; } =
        from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
        from openBracket in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.OpenBracket).Then(_ => PythonTypeDefinitionTokenizer.ManyDelimitedBy(Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Comma))).OptionalOrDefault()
        from closeBracket in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.CloseBracket).Optional()
        select new PythonTypeSpec { Name = name.ToStringValue(), Arguments = openBracket };

    // Returns the string without the quotes
    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, string> StringConstantTokenizer { get; } =
        from constant in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.String)
        select StringConstant.Parse(constant.Span.ToString());

    // Any constant value
    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, string> ConstantValueTokenizer { get; } =
        from constant in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Integer)
            .Or(Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Decimal))
            .Or(Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.String)).OptionalOrDefault()
        select constant.ToString(); // This possibly shouldn't be a string

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonFunctionParameter> PythonParameterTokenizer { get; } = 
        from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
        from colon in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Colon).Optional()
        from type in PythonTypeDefinitionTokenizer.OptionalOrDefault()
        from defaultValue in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Equal).Optional().Then(
                _ => ConstantValueTokenizer.OptionalOrDefault()
            )
        select new PythonFunctionParameter { Name = name.ToStringValue(), Type = type, DefaultValue = defaultValue };

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

    public static bool TryParseFunctionDefinitions(string source, out PythonFunctionDefinition[]? pythonSignatures, out string[] errors)
    {
        List<PythonFunctionDefinition> functionDefinitions = [];

        // Go line by line
        var lines = source.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        var currentErrors = new List<string>();
        foreach (var line in lines)
        {
            if (IsFunctionSignature(line))
            {
                // Parse the function signature
                var result = PythonSignatureTokenizer.Instance.TryTokenize(line);
                if (!result.HasValue)
                {
                    currentErrors.Add(result.ToString());
                    continue;
                }

                // TODO : Functions that span multiple lines

                var functionDefinition = PythonFunctionDefinitionTokenizer.TryParse(result.Value);
                if (functionDefinition.HasValue) {
                    functionDefinitions.Add(functionDefinition.Value);
                }
                else
                {
                    // Error parsing the function definition
                    currentErrors.Add(functionDefinition.ToString());
                }
            }
        }
        pythonSignatures = [.. functionDefinitions];
        errors = [.. currentErrors];
        return errors.Length == 0;
    }
}
