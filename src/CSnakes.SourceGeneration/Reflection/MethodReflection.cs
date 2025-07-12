using CSnakes.Parser.Types;
using CSnakes.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using CSharpParameterList = CSnakes.Parser.Types.PythonFunctionParameterList<Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax>;

namespace CSnakes.Reflection;


public static class MethodReflection
{
    public static MethodDefinition FromMethod(PythonFunctionDefinition function, string moduleName)
    {
        // Step 1: Determine the return type of the method
        PythonTypeSpec returnPythonType = function.ReturnType;

        ParameterSyntax? cancellationTokenParameterSyntax = null;
        const string cancellationTokenName = "cancellationToken";

        var doesReturnValue = returnPythonType.Name is not "None";

        var returnSyntax
            = doesReturnValue
            ? TypeReflection.AsPredefinedType(returnPythonType, TypeReflection.ConversionDirection.FromPython)
            : PredefinedType(Token(SyntaxKind.VoidKeyword));

        if (function.IsAsync)
        {
            cancellationTokenParameterSyntax =
                Parameter(Identifier(cancellationTokenName))
                    .WithType(IdentifierName("CancellationToken"))
                    .WithDefault(EqualsValueClause(LiteralExpression(SyntaxKind.DefaultLiteralExpression)));

            // If simple async type annotation, treat as Coroutine[T1, T2, T3] where T1 is the yield type, T2 is the send type and T3 is the return type
            if (returnPythonType.Name != "Coroutine")
            {
                returnPythonType = new PythonTypeSpec("Coroutine",
                    [
                        returnPythonType, // Yield type
                        PythonTypeSpec.None, // Send type
                        PythonTypeSpec.None, // Return type, TODO: Swap with yield on #480
                    ]);
            }

            returnSyntax = returnPythonType switch
            {
                { Name: "Coroutine", Arguments: [{ Name: "None" }, _, _] } =>
                    // Make it PyObject otherwise we need to annotate as Task instead of Task<T> and that adds a lot of complexity and little value
                    ParseTypeName("PyObject"),
                { Name: "Coroutine", Arguments: [var tYield, _, _] } =>
                    TypeReflection.AsPredefinedType(tYield, TypeReflection.ConversionDirection.FromPython),
                _ => throw new ArgumentException("Async function must return a Coroutine[T1, T2, T3]")
            };
            // return is a Task of <T> or Task when void
            returnSyntax
                = doesReturnValue
                ? GenericName(Identifier("Task")).WithTypeArgumentList(TypeArgumentList(SeparatedList([returnSyntax])))
                : IdentifierName("Task");
        }

        // Step 3: Build arguments
        var cSharpParameterList =
            function.Parameters.Map(ArgumentReflection.ArgumentSyntax,
                                    ArgumentReflection.ArgumentSyntax,
                                    p => ArgumentReflection.ArgumentSyntax(p, PythonFunctionParameterType.Star),
                                    ArgumentReflection.ArgumentSyntax,
                                    p => ArgumentReflection.ArgumentSyntax(p, PythonFunctionParameterType.DoubleStar));

        var parameterGenericArgs =
            cSharpParameterList.Enumerable()
                               .Select(p => p.Type)
                               .OfType<GenericNameSyntax>()
                               .ToList();

        // Step 4: Build body
        var pythonConversionStatements = new List<StatementSyntax>();
        foreach (var cSharpParameter in cSharpParameterList.WithVariadicPositional(null)
                                                           .WithVariadicKeyword(null)
                                                           .Enumerable())
        {
            bool needsConversion = true; // TODO: Skip .From for PyObject arguments.
            ExpressionSyntax rhs = IdentifierName(cSharpParameter.Identifier);
            if (needsConversion)
                rhs =
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("PyObject"),
                            IdentifierName("From")))
                        .WithArgumentList(
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(
                                        IdentifierName(cSharpParameter.Identifier)))));

            pythonConversionStatements.Add(
                LocalDeclarationStatement(
                    VariableDeclaration(
                        IdentifierName("PyObject"))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(
                                Identifier($"{cSharpParameter.Identifier}_pyObject"))
                            .WithInitializer(
                                EqualsValueClause(
                                    PostfixUnaryExpression(
                                        SyntaxKind.SuppressNullableWarningExpression,
                                        rhs
                                        ))))))
                .WithUsingKeyword(
                    Token(SyntaxKind.UsingKeyword)));
        }

        InvocationExpressionSyntax callExpression =
            InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("__underlyingPythonFunc"),
                    IdentifierName("Call")),
                function.Parameters switch
                {
                    // IF no *args, *kwargs or keyword-only args
                    { Keyword.IsEmpty: true, VariadicPositional: null, VariadicKeyword: null } =>
                        GenerateParamsCall(cSharpParameterList),
                    // IF *args but neither kwargs nor keyword-only arguments
                    { Keyword.IsEmpty: true, VariadicPositional: not null, VariadicKeyword: null } =>
                        GenerateArgsCall(cSharpParameterList),
                    var pyParameterList => GenerateKeywordCall(pyParameterList, cSharpParameterList)
                });

        ReturnStatementSyntax returnExpression;
        IEnumerable<StatementSyntax> resultConversionStatements = [];
        var callResultTypeSyntax = IdentifierName("PyObject");
        var returnNoneAsNull = false;
        var resultShouldBeDisposed = true;

        switch (returnSyntax)
        {
            case PredefinedTypeSyntax s when s.Keyword.IsKind(SyntaxKind.VoidKeyword):
            {
                returnExpression = ReturnStatement(null);
                break;
            }
            case IdentifierNameSyntax { Identifier.ValueText: "PyObject" }:
            {
                resultShouldBeDisposed = false;
                callResultTypeSyntax = IdentifierName("PyObject");
                returnExpression = ReturnStatement(IdentifierName("__result_pyObject"));
                break;
            }
            case NullableTypeSyntax:
            {
                returnNoneAsNull = true;
                // Assume `Optional[T]` and narrow to `T`
                returnPythonType = returnPythonType.Arguments[0];
                goto default;
            }
            default:
            {
                if (returnSyntax is GenericNameSyntax rg)
                    parameterGenericArgs.Add(rg);

                resultConversionStatements =
                    ResultConversionCodeGenerator.GenerateCode(returnPythonType,
                                                               "__result_pyObject", "__return",
                                                               cancellationTokenName);

                if (returnNoneAsNull)
                {
                    resultConversionStatements =
                        resultConversionStatements.Prepend(
                            IfStatement(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("__result_pyObject"),
                                        IdentifierName("IsNone"))),
                                ReturnStatement(
                                    LiteralExpression(SyntaxKind.NullLiteralExpression))
                            ));
                }

                returnExpression = ReturnStatement(IdentifierName("__return"));
                break;
            }
        }

        var functionObject = LocalDeclarationStatement(
            VariableDeclaration(
                IdentifierName("PyObject"))
            .WithVariables(
                SingletonSeparatedList(
                    VariableDeclarator(
                        Identifier("__underlyingPythonFunc"))
                    .WithInitializer(
                        EqualsValueClause(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                ThisExpression(),
                                IdentifierName($"__func_{function.Name}"))))))
            );

        StatementSyntax callStatement
            = returnExpression.Expression is not null
            ? LocalDeclarationStatement(
                  VariableDeclaration(
                          callResultTypeSyntax)
                  .WithVariables(
                      SingletonSeparatedList(
                          VariableDeclarator(
                              Identifier("__result_pyObject"))
                          .WithInitializer(
                              EqualsValueClause(
                                  callExpression)))))
                  .WithUsingKeyword(resultShouldBeDisposed ? Token(SyntaxKind.UsingKeyword) : Token(SyntaxKind.None))
            : ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, IdentifierName("_"), callExpression));

        var logStatement = ExpressionStatement(
                ConditionalAccessExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        IdentifierName("logger")),
                    InvocationExpression(
                        MemberBindingExpression(
                            IdentifierName("LogDebug")))
                        .WithArgumentList(
                            ArgumentList(
                                SeparatedList(
                                    [
                                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("Invoking Python function: {FunctionName}"))),
                                        Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(function.Name)))
                                    ])))
                        ));

        var body = Block(
            UsingStatement(
                null,
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("GIL"),
                        IdentifierName("Acquire"))),
                Block((StatementSyntax[])[
                    logStatement,
                    functionObject,
                    .. pythonConversionStatements,
                    callStatement,
                    .. resultConversionStatements,
                    returnExpression])
                ));

        // Sort the method parameters into this order
        // 1. All positional arguments
        // 2. All keyword-only arguments
        // 3. Any *args argument
        // 4. Any **kwargs argument
        var methodParameters =
            from ps in new[]
            {
                cSharpParameterList.Positional,
                cSharpParameterList.Regular,
                cSharpParameterList.Keyword,
                cSharpParameterList.VariadicPositional is { } vpd ? [vpd] : [],
                cSharpParameterList.VariadicKeyword is { } vkd ? [vkd] : [],
            }
            from p in ps
            select p;

        // Sort parameters to move all optional parameters to the end
        // CS1737: Optional parameters must appear after all required parameters
        methodParameters = methodParameters.OrderBy(
            p => p.Default is null ? 0 : 1
        );

        if (cancellationTokenParameterSyntax is { } someCancellationTokenParameterSyntax)
            methodParameters = methodParameters.Append(someCancellationTokenParameterSyntax);

        var syntax = MethodDeclaration(
            returnSyntax,
            Identifier(function.Name.ToPascalCase()))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithBody(body)
            .WithParameterList(ParameterList(SeparatedList(methodParameters)));

        return new(syntax);
    }

    private static ArgumentListSyntax GenerateParamsCall(CSharpParameterList parameterList) =>
        ArgumentList(SeparatedList(from a in parameterList.Positional.Concat(parameterList.Regular)
                                   select Argument(IdentifierName($"{a.Identifier}_pyObject"))));

    private static ArgumentListSyntax GenerateArgsCall(CSharpParameterList parameterList)
    {
        if (parameterList is not { VariadicPositional: { } vpp })
            throw new ArgumentException("Variadic positional parameter is required for *args call.", nameof(parameterList));

        var args =
            from a in parameterList.Positional.Concat(parameterList.Regular)
            select IdentifierName($"{a.Identifier}_pyObject");

        return ArgumentList(SeparatedList([Argument(CollectionExpression(SeparatedList(args.Select(CollectionElementSyntax (a) => ExpressionElement(a))))),
                                           Argument(IdentifierName(vpp.Identifier))]));
    }

    private static ArgumentListSyntax GenerateKeywordCall(PythonFunctionParameterList parameters,
                                                          CSharpParameterList reflectedParameters)
    {
        var args =
            from a in reflectedParameters.Positional.Concat(reflectedParameters.Regular)
            select IdentifierName($"{a.Identifier}_pyObject");

        IEnumerable<CollectionElementSyntax> kwargs =
            from a in parameters.Keyword.Zip(reflectedParameters.Keyword, (pp, rp) => (pp.Name, rp.Identifier))
            select ArgumentList(SeparatedList([Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(a.Name))),
                                               Argument(IdentifierName($"{a.Identifier}_pyObject"))]))
            into a
            select ExpressionElement(ImplicitObjectCreationExpression(a, null));

        return ArgumentList(SeparatedList([
                   Argument(CollectionExpression(SeparatedList(args.Select(CollectionElementSyntax (a) => ExpressionElement(a))))),
                   reflectedParameters.VariadicPositional is { } vpp
                       ? Argument(IdentifierName(vpp.Identifier))
                       : Argument(LiteralExpression(SyntaxKind.DefaultLiteralExpression)),
                   Argument(CollectionExpression(SeparatedList(kwargs))),
                   reflectedParameters.VariadicKeyword is { } vkp
                       ? Argument(IdentifierName(vkp.Identifier)) // TODO: The internal name might be mutated
                       : Argument(LiteralExpression(SyntaxKind.DefaultLiteralExpression))
               ]));
    }
}
