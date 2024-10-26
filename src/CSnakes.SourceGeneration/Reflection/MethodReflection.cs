using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSnakes.Parser.Types;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
        TypeSyntax? coroutineSyntax = null;

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
        } else
        {
            coroutineSyntax = TypeReflection.AsPredefinedType(returnPythonType, TypeReflection.ConversionDirection.FromPython);
            if (returnPythonType.Name != "Coroutine" || !returnPythonType.HasArguments() || returnPythonType.Arguments.Length != 3)
            {
                throw new ArgumentException("Async function must return a Coroutine[T1, T2, T3]");
            }
            var tYield = returnPythonType.Arguments[0];
            if (tYield.Name == "None")
            {
                // Make it PyObject otherwise we need to annotate as Task instead of Task<T> and that adds a lot of complexity and little value
                returnSyntax = ParseTypeName("PyObject");
            }
            else
            {
                returnSyntax = TypeReflection.AsPredefinedType(tYield, TypeReflection.ConversionDirection.FromPython);
            }
            // return is a Task of <T>
            returnSyntax = GenericName(Identifier("Task"))
                .WithTypeArgumentList(TypeArgumentList(SeparatedList([returnSyntax])));
        }

        // Step 3: Build arguments
        var parameterList = function.Parameters.ZipMap(ArgumentReflection.ArgumentSyntax);

        List<GenericNameSyntax> parameterGenericArgs = [];
        foreach (var (_, cSharpParameter) in parameterList)
        {
            if (cSharpParameter.Type is GenericNameSyntax g)
            {
                parameterGenericArgs.Add(g);
            }
        }

        // Step 4: Build body
        var pythonConversionStatements = new List<StatementSyntax>();
        foreach (var (pythonParameter, cSharpParameter) in parameterList)
        {
            if (pythonParameter is PythonFunctionParameter.Variadic)
            {
                continue;
            }
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

        InvocationExpressionSyntax? callExpression = null;

        // IF no *args, *kwargs or keyword-only args
        if (function.Parameters is { Keyword.IsEmpty: true, VariadicPositional: null, VariadicKeyword: null })
        {
            callExpression = GenerateParamsCall(parameterList);
        }
        else if (GenerateArgsCall(function, parameterList) is { } argsCallExpression)
        {
            // IF *args but no kwargs or keyword-only arguments
            callExpression = argsCallExpression;
        }
        else
        {
            // Support everything.
            callExpression = GenerateKeywordCall(function, parameterList);
        }

        ReturnStatementSyntax returnExpression = returnSyntax switch
        {
            GenericNameSyntax g when g.Identifier.Text == "Task" && coroutineSyntax is not null => ProcessAsyncMethodWithReturnType(coroutineSyntax, parameterGenericArgs),
            PredefinedTypeSyntax s when s.Keyword.IsKind(SyntaxKind.VoidKeyword) => ReturnStatement(null),
            IdentifierNameSyntax { Identifier.ValueText: "PyObject" } => ReturnStatement(IdentifierName("__result_pyObject")),
            _ => ProcessMethodWithReturnType(returnSyntax, parameterGenericArgs)
        };

        bool resultShouldBeDisposed = returnSyntax switch
        {
            PredefinedTypeSyntax s when s.Keyword.IsKind(SyntaxKind.VoidKeyword) => true,
            IdentifierNameSyntax => false,
            _ => true
        };

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

        LocalDeclarationStatementSyntax callStatement;

        if (!function.IsAsync)
        {
            callStatement = LocalDeclarationStatement(
                        VariableDeclaration(
                            IdentifierName("PyObject"))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier("__result_pyObject"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        callExpression)))));
            
        } else
        {
            callStatement = LocalDeclarationStatement(
                        VariableDeclaration(
                            IdentifierName("PyObject"))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier("__result_pyObject"))
                                )));
            
        }
        if (resultShouldBeDisposed && !function.IsAsync)
            callStatement = callStatement.WithUsingKeyword(Token(SyntaxKind.UsingKeyword));

        var logStatement = ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("logger"),
                        IdentifierName("LogDebug")))
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList(
                                [
                                    Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal("Invoking Python function: {FunctionName}"))),
                                    Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(function.Name)))
                                ])))
                    );
        
        BlockSyntax body;
        if (function.IsAsync)
        {
            ExpressionStatementSyntax localCallStatement = ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName("__result_pyObject"),
                    callExpression));

            StatementSyntax[] statements = [
                logStatement,
                functionObject,
                .. pythonConversionStatements,
                localCallStatement
                ];

            body = Block(
                callStatement,
                UsingStatement(
                    null,
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("GIL"),
                            IdentifierName("Acquire"))),
                    Block(statements)
                    ),
                returnExpression);
        } else
        {
            StatementSyntax[] statements = [
                logStatement,
                functionObject,
                .. pythonConversionStatements,
                callStatement,
                returnExpression];
            body = Block(
                UsingStatement(
                    null,
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("GIL"),
                            IdentifierName("Acquire"))),
                    Block(statements)
                    ));
        }
            
        // Sort the method parameters into this order
        // 1. All positional arguments
        // 2. All keyword-only arguments
        // 3. Any *args argument
        // 4. Any **kwargs argument
        var methodParameters =
            from ps in new[]
            {
                parameterList.PositionalData,
                parameterList.RegularData,
                parameterList.KeywordData,
                parameterList.TryGetVariadicPositional(out var vpd) ? [vpd] : [],
                parameterList.TryGetVariadicKeyword(out var vkd) ? [vkd] : [],
            }
            from p in ps
            select p;

        var modifiers = function.IsAsync ? TokenList(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.AsyncKeyword)
                )
            : TokenList(Token(SyntaxKind.PublicKeyword));

        var syntax = MethodDeclaration(
            returnSyntax,
            Identifier(function.Name.ToPascalCase()))
            .WithModifiers(modifiers)
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

    private static InvocationExpressionSyntax GenerateParamsCall(ParameterList<ParameterSyntax> parameterList)
    {
        return InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("__underlyingPythonFunc"),
                IdentifierName("Call")),
            ArgumentList(SeparatedList(from a in parameterList
                                       select Argument(IdentifierName($"{a.Data.Identifier}_pyObject")))));
    }

    private static InvocationExpressionSyntax? GenerateArgsCall(PythonFunctionDefinition function, ParameterList<ParameterSyntax> parameterList)
    {
        if (function.Parameters is not { VariadicPositional: { } vpp, Keyword.IsEmpty: true, VariadicKeyword: null })
        {
            return null;
        }

        // Merge the positional arguments and the *args into a single collection
        // Use CallWithArgs([arg1, arg2, ..args ?? []])
        var pythonFunctionCallArguments =
            parameterList.PositionalData.Concat(parameterList.RegularData)
                .Select((a) => Argument(IdentifierName($"{a.Identifier}_pyObject")))
               .ToList();

        var collection =
            SeparatedList<CollectionElementSyntax>(pythonFunctionCallArguments.Select((a) => ExpressionElement(a.Expression)))
            .Add(
                SpreadElement(
                    BinaryExpression(
                        SyntaxKind.CoalesceExpression,
                        IdentifierName(vpp.Name),
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

    private static InvocationExpressionSyntax GenerateKeywordCall(PythonFunctionDefinition function, ParameterList<ParameterSyntax> parameterList)
    {
        // Same as args, use a collection expression for all the positional args
        // [arg1, arg2, .. args ?? []]
        // Create a collection of string constants for the keyword-only names
        ;

        var collection =
            SeparatedList<CollectionElementSyntax>(from a in parameterList.PositionalData.Concat(parameterList.RegularData)
                                                   select ExpressionElement(IdentifierName($"{a.Identifier}_pyObject")));

        if (function.Parameters.VariadicPositional is { Name: var vpp })
        {
            collection = collection.Add(
                SpreadElement(
                    BinaryExpression(
                        SyntaxKind.CoalesceExpression,
                        IdentifierName(vpp),
                        CollectionExpression()
                    )
                )
            );
        }

        // Create a collection of string constants for the keyword-only names
        ArgumentSyntax kwnames = Argument(CollectionExpression(
            SeparatedList<CollectionElementSyntax>(
                from a in function.Parameters.Keyword
                select ExpressionElement(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(a.Name)))
            )));

        // Create a collection of the converted PyObject identifiers for keyword-only values
        ArgumentSyntax kwvalues = Argument(CollectionExpression(
            SeparatedList<CollectionElementSyntax>(
                from a in parameterList.KeywordData
                select ExpressionElement(IdentifierName($"{a.Identifier}_pyObject")))
                )
            );

        ArgumentSyntax kwargsArgument;
        // If there is a kwargs dictionary, add it to the arguments
        if (function.Parameters.VariadicKeyword is { Name: var vkp })
        {
            // TODO: The internal name might be mutated
            kwargsArgument = Argument(IdentifierName(vkp));
        }
        else
        {
            kwargsArgument = Argument(IdentifierName("null"));
        }

        ArgumentSyntax[] pythonFunctionCallArguments = [
            Argument(CollectionExpression(collection)),
            kwnames,
            kwvalues,
            kwargsArgument
            ];

        return InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("__underlyingPythonFunc"),
                        IdentifierName("CallWithKeywordArguments")),
                    ArgumentList(SeparatedList(pythonFunctionCallArguments)));
    }
}

file static class PythonFunctionParameterListExtensions
{
    public static ParameterList<T> ZipMap<T>(this PythonFunctionParameterList parameters, Func<PythonFunctionParameter, T> selector) =>
        new(parameters, [..from p in parameters select selector(p)]);
}

internal sealed class ParameterList<T>(PythonFunctionParameterList parameters, ImmutableArray<T> data) :
    IReadOnlyList<(PythonFunctionParameter Parameter, T Data)>
{
    private int PositionalIndex => 0;
    private int RegularIndex => parameters.Positional.Length;
    private int? VariadicPositionalIndex => parameters.VariadicPositional is not null ? RegularIndex + parameters.Regular.Length : null;
    private int KeywordIndex => VariadicPositionalIndex + 1 ?? RegularIndex + parameters.Regular.Length;
    private int? VariadicKeywordIndex => parameters.VariadicKeyword is not null ? KeywordIndex + parameters.Keyword.Length : null;

    public int Count => parameters.Count;

    public (PythonFunctionParameter Parameter, T Data) this[int index] => (parameters[index], data[index]);

    public IEnumerator<(PythonFunctionParameter Parameter, T Data)> GetEnumerator() =>
        parameters.Zip(data, ValueTuple.Create).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerable<T> PositionalData => data[PositionalIndex..RegularIndex];
    public IEnumerable<T> RegularData => data[RegularIndex..(VariadicPositionalIndex ?? KeywordIndex)];
    public IEnumerable<T> KeywordData => data[VariadicKeywordIndex is { } vki ? KeywordIndex..vki : KeywordIndex..];

    public bool TryGetVariadicPositional([NotNullWhen(true)] out T? result) =>
        TryGet(VariadicPositionalIndex, out result);

    public bool TryGetVariadicKeyword([NotNullWhen(true)] out T? result) =>
        TryGet(VariadicKeywordIndex, out result);

    private bool TryGet(int? index, [NotNullWhen(true)] out T? result)
    {
        if (index is { } i)
        {
            result = data[i];
            return true;
        }

        result = default;
        return false;
    }
}
