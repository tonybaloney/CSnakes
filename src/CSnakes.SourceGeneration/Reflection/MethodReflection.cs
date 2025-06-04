using CSnakes.Parser.Types;
using CSnakes.SourceGeneration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using CSharpParameterList = CSnakes.Parser.Types.PythonFunctionParameterList<Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax>;

namespace CSnakes.Reflection;
public class MethodDefinition(MethodDeclarationSyntax syntax, IEnumerable<GenericNameSyntax> parameterGenericArgs)
{
    public MethodDeclarationSyntax Syntax { get; } = syntax;

    public IEnumerable<GenericNameSyntax> ParameterGenericArgs { get; } = parameterGenericArgs;
}

public static class MethodReflection
{
    public static MethodDefinition FromMethod(PythonFunctionDefinition function, string moduleName)
    {
        // Step 1: Determine the return type of the method
        PythonTypeSpec returnPythonType = function.ReturnType;

        TypeSyntax returnSyntax;
        ParameterSyntax? cancellationTokenParameterSyntax = null;
        const string cancellationTokenName = "cancellationToken";

        if (!function.IsAsync)
        {
            if (returnPythonType.Name == "None")
            {
                returnSyntax = PredefinedType(Token(SyntaxKind.VoidKeyword));
            }
            else
            {
                returnSyntax = TypeReflection.AsPredefinedType(returnPythonType, TypeReflection.ConversionDirection.FromPython);
            }
        }
        else
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
                        new PythonTypeSpec("None", []), // Send type
                        new PythonTypeSpec("None", []) // Return type, TODO: Swap with yield on #480
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
            // return is a Task of <T>
            returnSyntax = GenericName(Identifier("Task"))
                .WithTypeArgumentList(TypeArgumentList(SeparatedList([returnSyntax])));
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

        InvocationExpressionSyntax callExpression = function.Parameters switch
        {
            // IF no *args, *kwargs or keyword-only args
            { Keyword.IsEmpty: true, VariadicPositional: null, VariadicKeyword: null } =>
                GenerateParamsCall(cSharpParameterList),
            // IF *args but neither kwargs nor keyword-only arguments
            { Keyword.IsEmpty: true, VariadicPositional: not null, VariadicKeyword: null } =>
                GenerateArgsCall(cSharpParameterList),
            _ => GenerateKeywordCall(function, cSharpParameterList)
        };

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
                    IdentifierName("logger"),
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

        if (cancellationTokenParameterSyntax is { } someCancellationTokenParameterSyntax)
            methodParameters = methodParameters.Append(someCancellationTokenParameterSyntax);

        var syntax = MethodDeclaration(
            returnSyntax,
            Identifier(function.Name.ToPascalCase()))
            .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
            .WithBody(body)
            .WithParameterList(ParameterList(SeparatedList(methodParameters)));

        return new(syntax, parameterGenericArgs);
    }

    private static ReturnStatementSyntax ProcessMethodWithReturnType(TypeSyntax returnSyntax, List<GenericNameSyntax> parameterGenericArgs)
    {
        ReturnStatementSyntax returnExpression;
        if (returnSyntax is GenericNameSyntax rg)
        {
            parameterGenericArgs.Add(rg);
        }

        returnExpression = ReturnStatement(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("__result_pyObject"),
                            GenericName(Identifier("As"))
                                .WithTypeArgumentList(TypeArgumentList(SeparatedList([returnSyntax])))))
                    .WithArgumentList(ArgumentList()));
        return returnExpression;
    }

    private static ReturnStatementSyntax ProcessAsyncMethodWithReturnType(TypeSyntax returnSyntax, List<GenericNameSyntax> parameterGenericArgs)
    {
        ReturnStatementSyntax returnExpression;
        if (returnSyntax is GenericNameSyntax rg)
        {
            parameterGenericArgs.Add(rg);
        }
        var pyObjectAs = InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("__result_pyObject"),
                            GenericName(Identifier("As"))
                                .WithTypeArgumentList(TypeArgumentList(SeparatedList([returnSyntax])))));

        returnExpression = ReturnStatement(
             AwaitExpression(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            pyObjectAs,
                            IdentifierName("AsTask")))
                    ));

        return returnExpression;
    }

    private static InvocationExpressionSyntax GenerateParamsCall(CSharpParameterList parameterList)
    {
        return InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("__underlyingPythonFunc"),
                IdentifierName("Call")),
            ArgumentList(SeparatedList(from a in parameterList.Enumerable()
                                       select Argument(IdentifierName($"{a.Identifier}_pyObject")))));
    }

    private static InvocationExpressionSyntax GenerateArgsCall(CSharpParameterList parameterList)
    {
        if (parameterList is not { VariadicPositional: { } vpp })
            throw new ArgumentException("Variadic positional parameter is required for *args call.", nameof(parameterList));

        // Merge the positional arguments and the *args into a single collection
        // Use CallWithArgs([arg1, arg2, ..args ?? []])
        var pythonFunctionCallArguments =
            parameterList.Positional.Concat(parameterList.Regular)
                .Select((a) => Argument(IdentifierName($"{a.Identifier}_pyObject")))
               .ToList();

        var collection =
            SeparatedList<CollectionElementSyntax>(pythonFunctionCallArguments.Select((a) => ExpressionElement(a.Expression)))
            .Add(
                SpreadElement(
                    BinaryExpression(
                        SyntaxKind.CoalesceExpression,
                        IdentifierName(vpp.Identifier),
                        CollectionExpression()
                    )
                )
            );
        pythonFunctionCallArguments = [Argument(CollectionExpression(collection))];

        return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("__underlyingPythonFunc"),
                        IdentifierName("CallWithArgs")),
                    ArgumentList(SeparatedList(pythonFunctionCallArguments)));
    }

    private static InvocationExpressionSyntax GenerateKeywordCall(PythonFunctionDefinition function, CSharpParameterList parameterList)
    {
        // Same as args, use a collection expression for all the positional args
        // [arg1, arg2, .. args ?? []]
        // Create a collection of string constants for the keyword-only names

        var collection =
            SeparatedList<CollectionElementSyntax>(from a in parameterList.Positional.Concat(parameterList.Regular)
                                                   select ExpressionElement(IdentifierName($"{a.Identifier}_pyObject")));

        if (parameterList.VariadicPositional is { } vpp)
        {
            collection = collection.Add(
                SpreadElement(
                    BinaryExpression(
                        SyntaxKind.CoalesceExpression,
                        IdentifierName(vpp.Identifier),
                        CollectionExpression()
                    )
                )
            );
        }

        // Create a collection of string constants for the keyword-only names
        var kwnames = CollectionExpression(
            SeparatedList<CollectionElementSyntax>(
                from a in function.Parameters.Keyword
                select ExpressionElement(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(a.Name)))
            ));

        // Create a collection of the converted PyObject identifiers for keyword-only values
        var kwvalues = CollectionExpression(
            SeparatedList<CollectionElementSyntax>(
                from a in parameterList.Keyword
                select ExpressionElement(IdentifierName($"{a.Identifier}_pyObject")))
                );

        var kwargsArgument =
            // If there is a kwargs dictionary, add it to the arguments
            parameterList.VariadicKeyword is { } vkp
            ? IdentifierName(vkp.Identifier) // TODO: The internal name might be mutated
            : IdentifierName("null");

        return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("__underlyingPythonFunc"),
                        IdentifierName("CallWithKeywordArguments")),
                    ArgumentList(SeparatedList([
                        Argument(CollectionExpression(collection)),
                        Argument(kwnames),
                        Argument(kwvalues),
                        Argument(kwargsArgument)
                    ])));
    }
}
