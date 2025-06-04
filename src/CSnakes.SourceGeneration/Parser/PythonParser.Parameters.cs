using CSnakes.Parser.Types;
using Superpower;
using Superpower.Parsers;

#pragma warning disable format

namespace CSnakes.Parser;
public static partial class PythonParser
{
    private static TokenListParser<PythonToken, PythonFunctionParameter>
        PythonParameterParser { get; } =
            (from name in Token.EqualTo(PythonToken.Identifier)
             from type in Token.EqualTo(PythonToken.Colon)
                               .IgnoreThen(PythonTypeDefinitionParser.AsNullable())
                               .OptionalOrDefault()
             select new PythonFunctionParameter(name.ToStringValue(), type, null))
           .Named("Parameter");

    public static TokenListParser<PythonToken, PythonFunctionParameter>
        OptionalPythonParameterParser { get; } =
            (from param in PythonParameterParser
             from defaultValue in Token.EqualTo(PythonToken.Equal)
                                       .IgnoreThen(ConstantValueTokenizer.AsNullable())
                                       .OptionalOrDefault()
             select param.WithDefaultValue(defaultValue))
            .Named("Parameter");

    public static TokenListParser<PythonToken, PythonFunctionParameterList> PythonParameterListParser { get; } =
        CreatePythonParameterListParser();

    private static TokenListParser<PythonToken, PythonFunctionParameterList> CreatePythonParameterListParser()
    {
        // In the interest of terseness and density, several short names are
        // used for variables to represent parameters throughout this method.
        // Below is the legend for these names:
        //
        // - p  : Parameter
        // - ps : Parameters or parameter list ("PythonFunctionParameterList")
        // - pps: Positional parameters
        // - vp : Variadic positional parameter ("*args")
        // - rps: Regular parameters (positional or keyword, but not variadic)
        // - ks : Keyword-only parameters
        // - vk : Variadic keyword parameter ("**kwargs")

        // For reference, the Python grammar for function parameters is as follows:
        //
        // parameter_list            ::=  defparameter ("," defparameter)* "," "/" ["," [parameter_list_no_posonly]]
        //                             |  parameter_list_no_posonly
        // parameter_list_no_posonly ::=  defparameter ("," defparameter)* ["," [parameter_list_starargs]]
        //                             |  parameter_list_starargs
        // parameter_list_starargs   ::=  "*" [star_parameter] ("," defparameter)* ["," ["**" parameter [","]]]
        //                             |  "**" parameter [","]
        // parameter_star_kwargs     ::= "**" parameter [","]
        // parameter                 ::= identifier [":" expression]
        // star_parameter            ::= identifier [":" ["*"] expression]
        // defparameter              ::= parameter ["=" expression]
        //
        // Source: https://docs.python.org/3/reference/compound_stmts.html#function-definitions

        // Parser for the comma token used throughout for parameter separation.

        var comma = Token.EqualTo(PythonToken.Comma);

        // Parser for parameters following a comma, matches sequences like:
        //
        //     defparameter ("," defparameter)*
        //
        // This handles parameters following another parameter or special syntax
        // element.

        var commaParametersParser =
            comma.IgnoreThen(OptionalPythonParameterParser.AtLeastOnceDelimitedBy(comma));

        // Parser for:
        //
        //     [star_parameter] ("," defparameter)*
        //
        // This handles two cases:
        //
        // 1. Variadic positional parameter followed by zero or more parameters
        //    that get designated as keyword-only parameters, e.g.: "*args,
        //    param1, param2, ...".
        //
        // 2. Bare "*" followed by zero or more parameters that get designated
        //    as keyword-only parameters, e.g.: "*, param1, param2, ...".
        //
        // This parser is designed to be composed with another parser that
        // handles the "*" token before "star_parameter".

        var starParametersParser =
            Parse.OneOf(// Case 1: "args, param1, param2, ..."
                        from vp in PythonParameterParser                       // variadic positional parameter (e.g., "*args")
                        from ks in commaParametersParser.OptionalOrDefault([]) // optional keyword-only parameters
                        select new PythonFunctionParameterList(varpos: vp, keyword: [..ks]),
                        // Case 2: ", param1, param2, ..."
                        from ks in commaParametersParser
                        select new PythonFunctionParameterList(keyword: [..ks]));

        // Parser that handles the ", **kwargs" syntax (variadic keyword
        // arguments) at the end of a parameter list, if present. To make sure
        // that the syntax is only allowed as the last in the parameter list, no
        // further parameter syntax parsers should be composed in sequence with
        // this parser.

        var optionalKwargsParameterParser =
            Token.EqualTo(PythonToken.CommaStarStar)
                 .IgnoreThen(PythonParameterParser.AsNullable())
                 .OptionalOrDefault();

        // Parser for keyword-only parameters appearing after ", *" and
        // optionally followed by a "**kwargs" parameter:
        //
        // ["," "*" [star_parameter] ("," defparameter)* ["," "**" parameter]]

        var keywordParametersParser =
            from ps in
                Token.EqualTo(PythonToken.CommaStar)                        // ",*" token
                     .IgnoreThen(starParametersParser)                      // Parse "*args" and/or keyword-only parameters
                     .OptionalOrDefault(PythonFunctionParameterList.Empty)  // If not present, use empty list
            from vk in optionalKwargsParameterParser                        // Optional "**kwargs" parameter
            select ps.WithVariadicKeyword(vk);                              // Combine into parameter list

        // Parser for the case of regular (positional or keyword) parameters
        // being followed by keyword-only parameters.

        var regularParametersParser =
            from rps in OptionalPythonParameterParser.ManyDelimitedBy(comma)  // Zero or more regular parameters
            from ps in keywordParametersParser                                // (Optional) keyword-only parameters
            select ps.WithRegular([..rps]);                                   // Combine into parameter list

        // Parser for positional-only parameters (those before "/"):
        //
        //     defparameter ("," defparameter)* "," "/" ...

        var positionalParametersParser =
            from pps in OptionalPythonParameterParser.AtLeastOnceDelimitedBy(comma) // One or more positional-only parameters
                                                     .ThenIgnore(Token.EqualTo(PythonToken.CommaSlash)) // Followed by "/"
            from ps in Parse.OneOf(comma.IgnoreThen(regularParametersParser), // Comma followed by regular parameters
                                   keywordParametersParser)                   // Or directly named args
            select ps.WithPositional([..pps]);                                // Add positional parameters to the list

        return Parse.OneOf(
                //
                // Case 1: Just a variadic keyword parameter:
                //
                //     "**kwargs"
                //
                from vk in Token.EqualTo(PythonToken.DoubleAsterisk)
                                 .IgnoreThen(PythonParameterParser)
                select new PythonFunctionParameterList(varkw: vk),
                //
                // Case 2: Starts with variadic positional parameter:
                //
                //     "*args[, param1, ...][, **kwargs])"
                //
                from ps in Token.EqualTo(PythonToken.Asterisk)
                                .IgnoreThen(starParametersParser)
                from vk in optionalKwargsParameterParser
                select ps.WithVariadicKeyword(vk),
                //
                // Case 3: Starts with positional parameters:
                //
                //     "param1, param2, ..., /[, ...]"
                //
                positionalParametersParser.Try(),
                //
                // Case 4: Regular parameters:
                //
                //     "param1, param2, ...[, *args, ...][, **kwargs]"
                //
                regularParametersParser)
                //
                // Parameter list in parentheses, while allowing for a trailing comma
                //
                .Between(Token.EqualTo(PythonToken.OpenParenthesis),
                         Token.EqualTo(PythonToken.CloseParenthesis).Or(Token.EqualTo(PythonToken.CommaCloseParenthesis))
                              .Named("`)`"))
                .Named("Parameter List")
                //
                // Validate that parameters follow Python's ordering rules:
                //
                .Where(ps => ps.Positional.Concat(ps.Regular) // Among all positional (/) and regular parameters...
                                          .Pairwise()    // ...check adjacent pairs, such that:
                                          .All(p => p is // both parameters are required (no default value)
                                                         ({ DefaultValue: null     }, { DefaultValue: null     })
                                                         // both parameters are optional (have default values)
                                                      or ({ DefaultValue: not null }, { DefaultValue: not null })
                                                         // first parameter is required, second is optional
                                                      or ({ DefaultValue: null     }, { DefaultValue: not null })),
                       "non-default argument follows default argument"); // Error message if validation fails
    }
}
