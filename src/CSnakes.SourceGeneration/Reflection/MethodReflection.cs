using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSnakes.Parser.Types;
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
                returnSyntax = PredefinedType(Token(SyntaxKind.VoidKeyword));
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
        List<(PythonFunctionParameter pythonParameter, ParameterSyntax cSharpParameter)> parameterList = ArgumentReflection.FunctionParametersAsParameterSyntaxPairs(function.Parameters);

        List<GenericNameSyntax> parameterGenericArgs = [];
        foreach (var (pythonParameter, cSharpParameter) in parameterList)
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
            if (pythonParameter.ParameterType != PythonFunctionParameterType.Normal)
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
        if (parameterList.All((a) => a.pythonParameter.ParameterType == PythonFunctionParameterType.Normal))
        {
            callExpression = GenerateParamsCall(parameterList);
        }
        else if (parameterList.Any((a) => a.pythonParameter.ParameterType == PythonFunctionParameterType.Star) &&
                 !parameterList.Any((a) => a.pythonParameter.ParameterType == PythonFunctionParameterType.DoubleStar) &&
                 !parameterList.Any((a) => a.pythonParameter.IsKeywordOnly))
        {
            // IF *args but no kwargs or keyword-only arguments
            callExpression = GenerateArgsCall(parameterList);
        }
        else
        {
            // Support everything.
            callExpression = GenerateKeywordCall(parameterList);
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
        var methodParameters = parameterList
            .Where((a) => a.pythonParameter.ParameterType == PythonFunctionParameterType.Normal && !a.pythonParameter.IsKeywordOnly)
            .Select((a) => a.cSharpParameter)
            .Concat(
                parameterList
                    .Where((a) => a.pythonParameter.ParameterType == PythonFunctionParameterType.Normal && a.pythonParameter.IsKeywordOnly)
                    .Select((a) => a.cSharpParameter)
            ).Concat(
                parameterList
                    .Where((a) => a.pythonParameter.ParameterType == PythonFunctionParameterType.Star)
                    .Select((a) => a.cSharpParameter)
            ).Concat(
                parameterList
                    .Where((a) => a.pythonParameter.ParameterType == PythonFunctionParameterType.DoubleStar)
                    .Select((a) => a.cSharpParameter)
            );

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

    private static InvocationExpressionSyntax GenerateParamsCall(IEnumerable<(PythonFunctionParameter pythonParameter, ParameterSyntax cSharpParameter)> parameterList)
    {
        IEnumerable<ArgumentSyntax> pythonFunctionCallArguments = parameterList.Select((a) => Argument(IdentifierName($"{a.cSharpParameter.Identifier}_pyObject"))).ToList();

        return InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("__underlyingPythonFunc"),
                IdentifierName("Call")),
            ArgumentList(SeparatedList(pythonFunctionCallArguments)));
    }

    private static InvocationExpressionSyntax GenerateArgsCall(IEnumerable<(PythonFunctionParameter pythonParameter, ParameterSyntax cSharpParameter)> parameterList)
    {
        // Merge the positional arguments and the *args into a single collection
        // Use CallWithArgs([arg1, arg2, ..args ?? []])
        IEnumerable<ArgumentSyntax> pythonFunctionCallArguments =
            parameterList
                .Where((arg) => arg.pythonParameter.ParameterType == PythonFunctionParameterType.Normal)
                .Select((a) => Argument(IdentifierName($"{a.cSharpParameter.Identifier}_pyObject"))).ToList();

        var (pythonParameter, cSharpParameter) = parameterList.First(p => p.pythonParameter.ParameterType == PythonFunctionParameterType.Star);
        SeparatedSyntaxList<CollectionElementSyntax> collection = SeparatedList<CollectionElementSyntax>()
            .AddRange(pythonFunctionCallArguments.Select((a) => ExpressionElement(a.Expression)))
            .Add(
                SpreadElement(
                    BinaryExpression(
                        SyntaxKind.CoalesceExpression,
                        IdentifierName(pythonParameter.Name),
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

    private static InvocationExpressionSyntax GenerateKeywordCall(IEnumerable<(PythonFunctionParameter pythonParameter, ParameterSyntax cSharpParameter)> parameterList)
    {
        // Same as args, use a collection expression for all the positional args
        // [arg1, arg2, .. args ?? []]
        // Create a collection of string constants for the keyword-only names
        IEnumerable<ArgumentSyntax> positionalArgumentsForCollection =
            parameterList
                .Where((arg) => arg.pythonParameter.ParameterType == PythonFunctionParameterType.Normal &&
                                !arg.pythonParameter.IsKeywordOnly)
                .Select((a) => Argument(IdentifierName($"{a.cSharpParameter.Identifier}_pyObject"))).ToList();

        SeparatedSyntaxList<CollectionElementSyntax> collection = SeparatedList<CollectionElementSyntax>()
            .AddRange(positionalArgumentsForCollection.Select((a) => ExpressionElement(a.Expression)));

        if (parameterList.Any((a) => a.pythonParameter.ParameterType == PythonFunctionParameterType.Star))
        {
            collection = collection.Add(
                SpreadElement(
                    BinaryExpression(
                        SyntaxKind.CoalesceExpression,
                        IdentifierName(parameterList.First(p => p.pythonParameter.ParameterType == PythonFunctionParameterType.Star).pythonParameter.Name),
                        CollectionExpression()
                    )
                )
            );
        }

        // Create a collection of string constants for the keyword-only names
        ArgumentSyntax kwnames = Argument(CollectionExpression(
            SeparatedList<CollectionElementSyntax>(
                parameterList.Where((arg) => arg.pythonParameter.IsKeywordOnly && arg.pythonParameter.ParameterType == PythonFunctionParameterType.Normal)
                    .Select(
                        (a) => ExpressionElement(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(a.pythonParameter.Name)))
                    )
                )
            ));

        // Create a collection of the converted PyObject identifiers for keyword-only values
        ArgumentSyntax kwvalues = Argument(CollectionExpression(
            SeparatedList<CollectionElementSyntax>(
                parameterList.Where((arg) => arg.pythonParameter.IsKeywordOnly && arg.pythonParameter.ParameterType == PythonFunctionParameterType.Normal)
                    .Select(
                        (a) => ExpressionElement(IdentifierName($"{a.cSharpParameter.Identifier}_pyObject")))
                    )
                )
            );

        ArgumentSyntax kwargsArgument;
        // If there is a kwargs dictionary, add it to the arguments
        if (parameterList.Any((a) => a.pythonParameter.ParameterType == PythonFunctionParameterType.DoubleStar))
        {
            // TODO: The internal name might be mutated
            kwargsArgument = Argument(IdentifierName(parameterList.First(p => p.pythonParameter.ParameterType == PythonFunctionParameterType.DoubleStar).pythonParameter.Name));
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
