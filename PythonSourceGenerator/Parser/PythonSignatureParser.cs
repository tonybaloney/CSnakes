using PythonSourceGenerator.Parser.Types;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PythonSourceGenerator.Parser;
public static class PythonSignatureParser
{
    public static bool IsFunctionSignature(string line)
    {
        // Check if the line starts with "def"
        return line.StartsWith("def ") || line.StartsWith("async def");
    }

    public static TextParser<Unit> IntegerConstantToken { get; } =
        from sign in Character.EqualTo('-').OptionalOrDefault()
        from digits in Character.Digit.AtLeastOnce()
        select Unit.Value;

    public static TextParser<Unit> DecimalConstantToken { get; } =
        from sign in Character.EqualTo('-').OptionalOrDefault()
        from digits in Character.Digit.Many().OptionalOrDefault(['0'])
        from decimal_ in Character.EqualTo('.')
        from rest in Character.Digit.Many()
        select Unit.Value;

    public static TextParser<Unit> DoubleQuotedStringConstantToken { get; } =
        from open in Character.EqualTo('"')
        from chars in Character.ExceptIn('"').Many()
        from close in Character.EqualTo('"')
        select Unit.Value;

    public static TextParser<Unit> SingleQuotedStringConstantToken { get; } =
        from open in Character.EqualTo('\'')
        from chars in Character.ExceptIn('\'').Many()
        from close in Character.EqualTo('\'')
        select Unit.Value;

    static class ConstantParsers
    {
        public static TextParser<string> DoubleQuotedString { get; } =
            from open in Character.EqualTo('"')
            from chars in Character.ExceptIn('"', '\\')
                .Or(Character.EqualTo('\\')
                    .IgnoreThen(
                        Character.EqualTo('\\')
                        .Or(Character.EqualTo('"'))
                        .Named("escape sequence")))
                .Many()
            from close in Character.EqualTo('"')
            select new string(chars);

        public static TextParser<string> SingleQuotedString { get; } =
            from open in Character.EqualTo('\'')
            from chars in Character.ExceptIn('\'', '\\')
                .Or(Character.EqualTo('\\')
                    .IgnoreThen(
                        Character.EqualTo('\\')
                        .Or(Character.EqualTo('\''))
                        .Named("escape sequence")))
                .Many()
            from close in Character.EqualTo('\'')
            select new string(chars);

        public static TextParser<int> Integer { get; } =
            from sign in Character.EqualTo('-').Value(-1).OptionalOrDefault(1)
            from whole in Numerics.Natural.Select(n => int.Parse(n.ToStringValue()))
            select whole * sign;

        // TODO: (track) This a copy from the JSON spec and probably doesn't reflect Python's other numeric literals like Hex and Real
        public static TextParser<double> Decimal { get; } =
            from sign in Character.EqualTo('-').Value(-1.0).OptionalOrDefault(1.0)
            from whole in Numerics.Natural.Select(n => double.Parse(n.ToStringValue()))
            from frac in Character.EqualTo('.')
                .IgnoreThen(Numerics.Natural)
                .Select(n => double.Parse(n.ToStringValue()) * Math.Pow(10, -n.Length))
                .OptionalOrDefault()
            from exp in Character.EqualToIgnoreCase('e')
                .IgnoreThen(Character.EqualTo('+').Value(1.0)
                    .Or(Character.EqualTo('-').Value(-1.0))
                    .OptionalOrDefault(1.0))
                .Then(expsign => Numerics.Natural.Select(n => double.Parse(n.ToStringValue()) * expsign))
                .OptionalOrDefault()
            select (whole + frac) * sign * Math.Pow(10, exp);
    }

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonTypeSpec> PythonTypeDefinitionTokenizer { get; } =
        (from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier).Or(Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.None))
         from openBracket in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.OpenBracket)
            .Then(_ => PythonTypeDefinitionTokenizer.ManyDelimitedBy(
                Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Comma),
                end: Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.CloseBracket)
                ))
            .OptionalOrDefault()
         select new PythonTypeSpec(name.ToStringValue(), openBracket))
        .Named("Type Definition");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> DoubleQuotedStringConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.DoubleQuotedString)
        .Apply(ConstantParsers.DoubleQuotedString)
        .Select(s => new PythonConstant { IsString = true, StringValue = s })
        .Named("Double Quoted String Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> SingleQuotedStringConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.SingleQuotedString)
        .Apply(ConstantParsers.SingleQuotedString)
        .Select(s => new PythonConstant { IsString = true, StringValue = s })
        .Named("Single Quoted String Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> DecimalConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Decimal)
        .Apply(ConstantParsers.Decimal)
        .Select(d => new PythonConstant { IsFloat = true, FloatValue = d })
        .Named("Decimal Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> IntegerConstantTokenizer { get; } =
        Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Integer)
        .Apply(ConstantParsers.Integer)
        .Select(d => new PythonConstant { IsInteger = true, IntegerValue = d })
        .Named("Integer Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> BoolConstantTokenizer { get; } =
    Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.True).Or(Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.False))
    .Select(d => new PythonConstant { IsBool = true, BoolValue = d.Kind == PythonSignatureTokens.PythonSignatureToken.True })
    .Named("Bool Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> NoneConstantTokenizer { get; } =
    Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.None)
    .Select(d => new PythonConstant { IsNone = true })
    .Named("None Constant");

    // Any constant value
    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonConstant> ConstantValueTokenizer { get; } =
        DecimalConstantTokenizer.AsNullable()
        .Or(IntegerConstantTokenizer.AsNullable())
        .Or(BoolConstantTokenizer.AsNullable())
        .Or(NoneConstantTokenizer.AsNullable())
        .Or(DoubleQuotedStringConstantTokenizer.AsNullable())
        .Or(SingleQuotedStringConstantTokenizer.AsNullable())
        .Named("Constant");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonFunctionParameter> PythonParameterTokenizer { get; } =
        (
        from star in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Asterisk).Optional()
        from doubleStar in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.DoubleAsterisk).Optional()
        from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
        from colon in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Colon).Optional()
        from type in PythonTypeDefinitionTokenizer.OptionalOrDefault()
        from defaultValue in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Equal).Optional().Then(
                _ => ConstantValueTokenizer.OptionalOrDefault()
            )
        select new PythonFunctionParameter(name.ToStringValue(), type, defaultValue, star is not null, doubleStar is not null))
        .Named("Parameter");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonFunctionParameter[]> PythonParameterListTokenizer { get; } =
        (from openParen in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.OpenParenthesis)
         from parameters in PythonParameterTokenizer.ManyDelimitedBy(Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Comma))
         from closeParen in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.CloseParenthesis)
         select parameters)
        .Named("Parameter List");

    public static TokenListParser<PythonSignatureTokens.PythonSignatureToken, PythonFunctionDefinition> PythonFunctionDefinitionTokenizer { get; } =
        (from def in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Def)
         from name in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Identifier)
         from parameters in PythonParameterListTokenizer
         from arrow in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Arrow).Optional().Then(returnType => PythonTypeDefinitionTokenizer.OptionalOrDefault())
         from colon in Token.EqualTo(PythonSignatureTokens.PythonSignatureToken.Colon)
         select new PythonFunctionDefinition(name.ToStringValue(), arrow, parameters))
        .Named("Function Definition");

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
