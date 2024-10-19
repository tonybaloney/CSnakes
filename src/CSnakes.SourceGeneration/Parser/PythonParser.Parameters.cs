using CSnakes.Parser.Types;
using Superpower;
using Superpower.Parsers;

namespace CSnakes.Parser;
public static partial class PythonParser
{
    public static TokenListParser<PythonToken, (string Name, PythonTypeSpec? Type)> BasePythonParameterParser { get; } =
        (from arg in PythonNormalArgParser
         from type in Token.EqualTo(PythonToken.Colon)
                           .IgnoreThen(PythonTypeDefinitionParser.AssumeNotNull())
                           .OptionalOrDefault()
         select (arg.Name, type))
       .Named("Parameter");

    public static TokenListParser<PythonToken, PythonFunctionParameter> PythonParameterParser { get; } =
        (from param in BasePythonParameterParser
         from defaultValue in Token.EqualTo(PythonToken.Equal)
                                   .IgnoreThen(ConstantValueTokenizer.AssumeNotNull())
                                   .OptionalOrDefault()
         select (PythonFunctionParameter)new PythonFunctionParameter.Normal(param.Name, param.Type, defaultValue))
        .Named("Parameter");

    public static TokenListParser<PythonToken, PythonFunctionParameterList> PythonParameterListParser { get; } =
        CreatePythonParameterListParser();

    public static TokenListParser<PythonToken, PythonFunctionParameterList> CreatePythonParameterListParser()
    {
        /*
         parameter_list            ::=  defparameter ("," defparameter)* "," "/" ["," [parameter_list_no_posonly]]
                                     |  parameter_list_no_posonly
         parameter_list_no_posonly ::=  defparameter ("," defparameter)* ["," [parameter_list_starargs]]
                                     |  parameter_list_starargs
         parameter_list_starargs   ::=  "*" [star_parameter] ("," defparameter)* ["," ["**" parameter [","]]]
                                     |  "**" parameter [","]
         */

        var comma = Token.EqualTo(PythonToken.Comma);

        var commaParameters =
            comma.IgnoreThen(PythonParameterParser.AtLeastOnceDelimitedBy(comma));

        var optionalKwargParameterParser =
            Token.EqualTo(PythonToken.CommaStarStar)
                 .IgnoreThen(BasePythonParameterParser.Select(p => new PythonFunctionParameter.DoubleStar(p.Name, p.Type))
                                                      .AsNullable())
                 .OptionalOrDefault();

        var starParameters =
            Parse.OneOf(from pp in BasePythonParameterParser
                        from kp in commaParameters.OptionalOrDefault([])
                        select new PythonFunctionParameterList(varpos: new PythonFunctionParameter.Star(pp.Name, pp.Type), keyword: [.. kp]),
                        from kp in commaParameters
                        select new PythonFunctionParameterList(keyword: [.. kp]));

        var namedArgParameterParser =
            from namedArgParameters in
                Token.EqualTo(PythonToken.CommaStar)
                     .IgnoreThen(starParameters)
                     .OptionalOrDefault(PythonFunctionParameterList.Empty)
            from kwargParameter in optionalKwargParameterParser
            select namedArgParameters.WithVariadicKeyword(kwargParameter);

        var b =
            from rps in PythonParameterParser.ManyDelimitedBy(comma)
            from ps in namedArgParameterParser
            select ps.WithRegular([.. rps]);

        // ( arg "," )+ ", /" ( ( "," arg )* ( ", *" ( "," arg )+ )? )?
        var a =
            from pps in PythonParameterParser.AtLeastOnceDelimitedBy(comma)
                                             .ThenIgnore(Token.EqualTo(PythonToken.CommaSlash))
            from ps in Parse.OneOf(comma.IgnoreThen(b), namedArgParameterParser)
            select ps.WithPositional([.. pps]);

        return Parse.OneOf(// "**" ...
                           from vkp in Token.EqualTo(PythonToken.DoubleAsterisk)
                                            .IgnoreThen(BasePythonParameterParser)
                           select new PythonFunctionParameterList(varkw: new PythonFunctionParameter.DoubleStar(vkp.Name, vkp.Type)),
                           // "*" ...
                           from kp in Token.EqualTo(PythonToken.Asterisk)
                                           .IgnoreThen(starParameters)
                           from vkp in optionalKwargParameterParser
                           select kp.WithVariadicKeyword(vkp),//new PythonFunctionParameterList(keyword: [..kp], varkw: vkp),
                                                              // ( arg "," )+ "/" ...
                           a.Try(),
                           b)
                    .Between(Token.EqualTo(PythonToken.OpenParenthesis),
                             Token.EqualTo(PythonToken.CloseParenthesis).Or(Token.EqualTo(PythonToken.CommaCloseParenthesis)))
                    .Named("Parameter List");
    }
}
