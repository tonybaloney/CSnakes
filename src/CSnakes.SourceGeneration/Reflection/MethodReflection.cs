using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSnakes.Parser.Types;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
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
        var cSharpParameterList =
            function.Parameters.Map(ArgumentReflection.ArgumentSyntax,
                                    ArgumentReflection.ArgumentSyntax,
                                    p => ArgumentReflection.ArgumentSyntax(p, PythonFunctionParameterType.Star),
                                    ArgumentReflection.ArgumentSyntax,
                                    p => ArgumentReflection.ArgumentSyntax(p, PythonFunctionParameterType.DoubleStar));

        List<GenericNameSyntax> parameterGenericArgs = [];
        foreach (var cSharpParameter in cSharpParameterList.Enumerable())
        {
            if (cSharpParameter.Type is GenericNameSyntax g)
            {
                parameterGenericArgs.Add(g);
            }
        }

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

        InvocationExpressionSyntax? callExpression = null;

        // IF no *args, *kwargs or keyword-only args
        if (function.Parameters is { Keyword.IsEmpty: true, VariadicPositional: null, VariadicKeyword: null })
        {
            callExpression = GenerateParamsCall(cSharpParameterList);
        }
        else if (GenerateArgsCall(function, cSharpParameterList) is { } argsCallExpression)
        {
            // IF *args but no kwargs or keyword-only arguments
            callExpression = argsCallExpression;
        }
        else
        {
            // Support everything.
            callExpression = GenerateKeywordCall(function, cSharpParameterList);
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
                cSharpParameterList.Positional,
                cSharpParameterList.Regular,
                cSharpParameterList.Keyword,
                cSharpParameterList.VariadicPositional is { } vpd ? [vpd] : [],
                cSharpParameterList.VariadicKeyword is {} vkd ? [vkd] : [],
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

    private static InvocationExpressionSyntax? GenerateArgsCall(PythonFunctionDefinition function, CSharpParameterList parameterList)
    {
        if (function.Parameters is not { VariadicPositional: { } vpp, Keyword.IsEmpty: true, VariadicKeyword: null })
        {
            return null;
        }

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

    private static InvocationExpressionSyntax GenerateKeywordCall(PythonFunctionDefinition function, CSharpParameterList parameterList)
    {
        // Same as args, use a collection expression for all the positional args
        // [arg1, arg2, .. args ?? []]
        // Create a collection of string constants for the keyword-only names

        var collection =
            SeparatedList<CollectionElementSyntax>(from a in parameterList.Positional.Concat(parameterList.Regular)
                                                   select ExpressionElement(IdentifierName($"{a.Identifier}_pyObject")));

        if (function.Parameters.VariadicPositional is { } vpp)
        {
            collection = collection.Add(
                SpreadElement(
                    BinaryExpression(
                        SyntaxKind.CoalesceExpression,
                        IdentifierName(vpp.Name),
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
                from a in parameterList.Keyword
                select ExpressionElement(IdentifierName($"{a.Identifier}_pyObject")))
                )
            );

        ArgumentSyntax kwargsArgument;
        // If there is a kwargs dictionary, add it to the arguments
        if (function.Parameters.VariadicKeyword is { } vkp)
        {
            // TODO: The internal name might be mutated
            kwargsArgument = Argument(IdentifierName(vkp.Name));
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
