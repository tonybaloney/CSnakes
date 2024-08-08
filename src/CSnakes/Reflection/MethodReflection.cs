using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PythonSourceGenerator.Parser.Types;

namespace PythonSourceGenerator.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
public class MethodDefinition(MethodDeclarationSyntax syntax, IEnumerable<GenericNameSyntax> parameterGenericArgs)
{
    public MethodDeclarationSyntax Syntax { get; } = syntax;

    public IEnumerable<GenericNameSyntax> ParameterGenericArgs { get; } = parameterGenericArgs;
}

public static class MethodReflection
{
    public static MethodDefinition FromMethod(PythonFunctionDefinition function, string moduleName)
    {
        // Step 1: Create a method declaration

        // Step 2: Determine the return type of the method
        PythonTypeSpec returnPythonType = function.ReturnType;

        TypeSyntax returnSyntax;
        if (returnPythonType.Name == "None")
        {
            returnSyntax = PredefinedType(Token(SyntaxKind.VoidKeyword));
        }
        else
        {
            returnSyntax = TypeReflection.AsPredefinedType(returnPythonType);
        }

        // Step 3: Build arguments
        var parameterList = ArgumentReflection.ParameterListSyntax(function.Parameters);

        List<GenericNameSyntax> parameterGenericArgs = [];
        foreach (var genericType in parameterList.Parameters)
        {
            if (genericType.Type is GenericNameSyntax g)
            {
                parameterGenericArgs.Add(g);
            }
        }

        // Step 4: Build body
        var pythonConversionStatements = new List<StatementSyntax>();
        foreach (var parameter in parameterList.Parameters)
        {
            pythonConversionStatements.Add(
                LocalDeclarationStatement(
                        VariableDeclaration(
                            IdentifierName("PyObject"))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier($"{parameter.Identifier}_pyObject"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        BinaryExpression(
                                        SyntaxKind.AsExpression,
                                        InvocationExpression(
                                            MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            ThisExpression(),
                                            IdentifierName(
                                                Identifier(
                                                    TriviaList(),
                                                    SyntaxKind.ModuleKeyword,
                                                    "td",
                                                    "td",
                                                    TriviaList()))),
                                        IdentifierName("ConvertFrom")),
                                            ArgumentList(
                                                 SingletonSeparatedList(
                                        Argument(IdentifierName(parameter.Identifier))))),
                                        Token(SyntaxKind.AsKeyword),
                                IdentifierName("PyObject")))
                                ))))
                .WithUsingKeyword(
                    Token(SyntaxKind.UsingKeyword)));
        }

        var pythonCastArguments = new List<ArgumentSyntax>();
        foreach (var parameter in parameterList.Parameters)
        {
            pythonCastArguments.Add(Argument(IdentifierName($"{parameter.Identifier}_pyObject")));
        }

        ReturnStatementSyntax returnExpression = returnSyntax switch
        {
            TypeSyntax s when s is PredefinedTypeSyntax p && p.Keyword.IsKind(SyntaxKind.VoidKeyword) => ReturnStatement(null),
            TypeSyntax s when s is IdentifierNameSyntax => ReturnStatement(IdentifierName("__result_pyObject")),
            _ => ProcessMethodWithReturnType(returnSyntax, parameterGenericArgs)
        };

        var moduleDefinition = LocalDeclarationStatement(
                        VariableDeclaration(
                            IdentifierName("var"))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier("__underlyingPythonFunc"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        InvocationExpression(
                                            MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            ThisExpression(),
                                            IdentifierName(
                                                Identifier(
                                                    TriviaList(),
                                                    SyntaxKind.ModuleKeyword,
                                                    "module",
                                                    "module",
                                                    TriviaList()))),
                                        IdentifierName("GetAttr")),
                                            ArgumentList(
                                                SingletonSeparatedList(
                                                    Argument(
                                                        LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            Literal(function.Name)))))))))))
            .WithUsingKeyword(
                    Token(SyntaxKind.UsingKeyword));
        var callStatement = LocalDeclarationStatement(
                        VariableDeclaration(
                            IdentifierName("var"))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier("__result_pyObject"))
                                .WithInitializer(
                                    EqualsValueClause(
                                        InvocationExpression(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName("__underlyingPythonFunc"),
                                                IdentifierName("Call")),
                                            ArgumentList(
                                                SeparatedList(pythonCastArguments))))))))
            .WithUsingKeyword(
                    Token(SyntaxKind.UsingKeyword));
        StatementSyntax[] statements = [
            moduleDefinition,
            .. pythonConversionStatements,
            callStatement,
            // TODO : Add free statements
            returnExpression];
        var body = Block(
            UsingStatement(
                null,
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("GIL"),
                        IdentifierName("Acquire"))),
                Block(statements)
                ));

        var syntax = MethodDeclaration(
            returnSyntax,
            Identifier(function.Name.ToPascalCase()))
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword))
                )
            .WithBody(body)
            .WithParameterList(parameterList);

        return new(syntax, parameterGenericArgs);
    }

    private static ReturnStatementSyntax ProcessMethodWithReturnType(TypeSyntax returnSyntax, List<GenericNameSyntax> parameterGenericArgs)
    {
        ReturnStatementSyntax returnExpression;
        if (returnSyntax is GenericNameSyntax rg)
        {
            parameterGenericArgs.Add(rg);
        }

        var converter =
            GenericName(
                Identifier("As"))
            .WithTypeArgumentList(
                TypeArgumentList(
                    SeparatedList([returnSyntax])));

        returnExpression = ReturnStatement(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("__result_pyObject"),
                            converter))
                    .WithArgumentList(
                        ArgumentList()));
        return returnExpression;
    }
}
