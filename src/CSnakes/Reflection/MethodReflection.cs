using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PythonSourceGenerator.Parser.Types;

namespace PythonSourceGenerator.Reflection;

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
            returnSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));
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

        // Import module
        // var mod = Py.Import("hello_world");
        var moduleLoad = SyntaxFactory.LocalDeclarationStatement(
            SyntaxFactory.VariableDeclaration(
                SyntaxFactory.IdentifierName("var"))
            .WithVariables(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.VariableDeclarator(
                        SyntaxFactory.Identifier("mod"))
                    .WithInitializer(
                        SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("Import"),
                                    SyntaxFactory.IdentifierName("ImportModule")),
                                SyntaxFactory.ArgumentList(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.Argument(
                                            SyntaxFactory.LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                SyntaxFactory.Literal(moduleName)))))))))));

        // Step 4: Build body
        var pythonCastArguments = new List<ArgumentSyntax>();
        foreach (var parameter in parameterList.Parameters)
        {
            pythonCastArguments.Add(SyntaxFactory.Argument(SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName(parameter.Identifier),
                                                SyntaxFactory.IdentifierName("ToPython")))));
        }

        ReturnStatementSyntax returnExpression = returnSyntax switch
        {
            TypeSyntax s when s is PredefinedTypeSyntax p && p.Keyword.IsKind(SyntaxKind.VoidKeyword) => SyntaxFactory.ReturnStatement(null),
            TypeSyntax s when s is IdentifierNameSyntax => SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName("result")),
            _ => ProcessMethodWithReturnType(returnSyntax, parameterGenericArgs)
        };

        var body = SyntaxFactory.Block(
            moduleLoad,
            SyntaxFactory.UsingStatement(
                null,
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName("Py"),
                        SyntaxFactory.IdentifierName("GIL"))),
                SyntaxFactory.Block(
                    SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.IdentifierName("var"))
                        .WithVariables(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier("func"))
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName("mod"),
                                                SyntaxFactory.IdentifierName("GetAttr")),
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.LiteralExpression(
                                                            SyntaxKind.StringLiteralExpression,
                                                            SyntaxFactory.Literal(function.Name))))))))))),
                    SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.IdentifierName("var"))
                        .WithVariables(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.VariableDeclarator(
                                    SyntaxFactory.Identifier("result"))
                                .WithInitializer(
                                    SyntaxFactory.EqualsValueClause(
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName("func"),
                                                SyntaxFactory.IdentifierName("Call")),
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SeparatedList(pythonCastArguments)))))))),
                    returnExpression
                    )

                ));

        var syntax = SyntaxFactory.MethodDeclaration(
            returnSyntax,
            SyntaxFactory.Identifier(function.Name.ToPascalCase()))
            .WithModifiers(
                SyntaxFactory.TokenList(
                    SyntaxFactory.Token(SyntaxKind.PublicKeyword))
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

        var converter = SyntaxFactory
            .GenericName(
                SyntaxFactory.Identifier("As"))
            .WithTypeArgumentList(
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SeparatedList([returnSyntax])));

        returnExpression = SyntaxFactory.ReturnStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName("result"),
                            converter))
                    .WithArgumentList(
                        SyntaxFactory.ArgumentList()));
        return returnExpression;
    }
}
