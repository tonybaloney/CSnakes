using CSnakes.Parser.Types;
using Superpower;
using Superpower.Parsers;

namespace CSnakes.Parser;
public static partial class PythonParser
{
    private static TokenListParser<PythonToken, PythonFunctionParameter>
        PythonParameterParser { get; } =
            (from name in Token.EqualTo(PythonToken.Identifier)
             from type in Token.EqualTo(PythonToken.Colon)
                               .IgnoreThen(PythonTypeDefinitionParser.AssumeNotNull())
                               .OptionalOrDefault()
             select new PythonFunctionParameter(name.ToStringValue(), type, null))
           .Named("Parameter");

    public static TokenListParser<PythonToken, PythonFunctionParameter>
        OptionalPythonParameterParser { get; } =
            (from param in PythonParameterParser
             from defaultValue in Token.EqualTo(PythonToken.Equal)
                                       .IgnoreThen(ConstantValueTokenizer)
                                       .OptionalOrDefault()
             select param.WithDefaultValue(defaultValue))
            .Named("Parameter");

    public static TokenListParser<PythonToken, PythonFunctionParameterList> PythonParameterListParser { get; } =
        CreatePythonParameterListParser();

    private static TokenListParser<PythonToken, PythonFunctionParameterList> CreatePythonParameterListParser()
    {
        /*
         parameter_list            ::=  defparameter ("," defparameter)* "," "/" ["," [parameter_list_no_posonly]]
                                     |  parameter_list_no_posonly
         parameter_list_no_posonly ::=  defparameter ("," defparameter)* ["," [parameter_list_starargs]]
                                     |  parameter_list_starargs
         parameter_list_starargs   ::=  "*" [star_parameter] ("," defparameter)* ["," ["**" parameter [","]]]
                                     |  "**" parameter [","]
         parameter_star_kwargs     ::= "**" parameter [","]
         parameter                 ::= identifier [":" expression]
         star_parameter            ::= identifier [":" ["*"] expression]
         defparameter              ::= parameter ["=" expression]
         */

        var comma = Token.EqualTo(PythonToken.Comma);

        var commaParametersParser =
            comma.IgnoreThen(OptionalPythonParameterParser.AtLeastOnceDelimitedBy(comma));

        var optionalKwargParameterParser =
            Token.EqualTo(PythonToken.CommaStarStar)
                 .IgnoreThen(PythonParameterParser.AsNullable())
                 .OptionalOrDefault();

        var starParametersParser =
            Parse.OneOf(from vp in PythonParameterParser
                        from ks in commaParametersParser.OptionalOrDefault([])
                        select new PythonFunctionParameterList(varpos: vp, keyword: [..ks]),
                        from ks in commaParametersParser
                        select new PythonFunctionParameterList(keyword: [..ks]));

        var namedArgParameterParser =
            from namedArgParameters in
                Token.EqualTo(PythonToken.CommaStar)
                     .IgnoreThen(starParametersParser)
                     .OptionalOrDefault(PythonFunctionParameterList.Empty)
            from kwargParameter in optionalKwargParameterParser
            select namedArgParameters.WithVariadicKeyword(kwargParameter);

        var b =
            from rps in OptionalPythonParameterParser.ManyDelimitedBy(comma)
            from ps in namedArgParameterParser
            select ps.WithRegular([.. rps]);

        // ( arg "," )+ ", /" ( ( "," arg )* ( ", *" ( "," arg )+ )? )?
        var a =
            from pps in OptionalPythonParameterParser.AtLeastOnceDelimitedBy(comma)
                                                     .ThenIgnore(Token.EqualTo(PythonToken.CommaSlash))
            from ps in Parse.OneOf(comma.IgnoreThen(b), namedArgParameterParser)
            select ps.WithPositional([.. pps]);

        return Parse.OneOf(// "**" ...
                           from vkp in Token.EqualTo(PythonToken.DoubleAsterisk)
                                            .IgnoreThen(PythonParameterParser)
                           select new PythonFunctionParameterList(varkw: vkp),
                           // "*" ...
                           from kps in Token.EqualTo(PythonToken.Asterisk)
                                            .IgnoreThen(starParametersParser)
                           from vkp in optionalKwargParameterParser
                           select kps.WithVariadicKeyword(vkp),
                           // ( arg "," )+ "/" ...
                           a.Try(),
                           b)
                    .Between(Token.EqualTo(PythonToken.OpenParenthesis),
                             Token.EqualTo(PythonToken.CloseParenthesis).Or(Token.EqualTo(PythonToken.CommaCloseParenthesis))
                                  .Named("`)`"))
                    .Named("Parameter List");
    }
}
