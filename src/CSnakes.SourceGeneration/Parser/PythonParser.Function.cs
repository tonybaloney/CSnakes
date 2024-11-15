﻿using Microsoft.CodeAnalysis.Text;
using CSnakes.Parser.Types;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

using ParsedTokens = Superpower.Model.TokenList<CSnakes.Parser.PythonToken>;
using System.Collections.Immutable;

namespace CSnakes.Parser;
public static partial class PythonParser
{
    public static TokenListParser<PythonToken, PythonFunctionDefinition> PythonFunctionDefinitionParser { get; } =
        (from @async in Token.EqualTo(PythonToken.Async).OptionalOrDefault()
         from def in Token.EqualTo(PythonToken.Def)
         from name in Token.EqualTo(PythonToken.Identifier)
         from parameters in PythonParameterListParser.AssumeNotNull()
         from arrow in Token.EqualTo(PythonToken.Arrow).Optional().Then(returnType => PythonTypeDefinitionParser.AssumeNotNull().OptionalOrDefault())
         from colon in Token.EqualTo(PythonToken.Colon)
         select new PythonFunctionDefinition(name.ToStringValue(), arrow, parameters, async.HasValue))
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
        List<(ImmutableArray<TextLine> lines, ParsedTokens tokens)> functionLines = [];
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
            Result<ParsedTokens> result = PythonTokenizer.Instance.TryTokenize(lineOfCode);
            if (!result.HasValue)
            {
                currentErrors.Add(new(
                    line.LineNumber,
                    line.LineNumber,
                    result.ErrorPosition.Column,
                    result.ErrorPosition.Column + result.Location.Length,
                    result.FormatErrorMessageFragment())
                );

                // Reset buffer
                currentBuffer = [];
                unfinishedFunctionSpec = false;
                continue;
            }

            ParsedTokens repositionedTokens = new(result.Value.Select(token =>
            {
                Superpower.Model.TextSpan span = new(token.Span.Source!, new(token.Span.Position.Absolute, line.LineNumber, token.Span.Position.Column), token.Span.Length);
                Token<PythonToken> t = new(token.Kind, span);
                return t;
            }).ToArray());
            currentBuffer.Add((line, repositionedTokens));

            // If this is a function definition on one line..
            if (repositionedTokens.Last().Kind == PythonToken.Colon)
            {
                ParsedTokens combinedTokens = new(currentBuffer.SelectMany(x => x.tokens).ToArray());

                functionLines.Add(([..from x in currentBuffer select x.line], combinedTokens));
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

        foreach (var (currentLines, tokens) in functionLines)
        {
            switch (PythonFunctionDefinitionParser.TryParse(tokens))
            {
                case { HasValue: true, Value: var functionDefinition }:
                    functionDefinitions.Add(functionDefinition.WithSourceLines(currentLines));
                    break;
                case var result:
                    // Error parsing the function definition
                    int lineNumber = currentLines.First().LineNumber;
                    currentErrors.Add(new(
                        lineNumber,
                        lineNumber + result.ErrorPosition.Line - 1,
                        result.ErrorPosition.Column,
                        result.ErrorPosition.Column + 1,
                        result.FormatErrorMessageFragment())
                    );
                    break;
            }
        }

        pythonSignatures = [.. functionDefinitions];
        errors = [.. currentErrors];
        return errors.Length == 0;
    }
}
