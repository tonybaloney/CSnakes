using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Python.Runtime;
using System.Collections.Generic;

namespace PythonSourceGenerator.Reflection
{
    public static class MethodReflection
    {
        public static MethodDeclarationSyntax FromMethod(PyObject signature, string methodName, string moduleName)
        {
            // Step 1: Create a method declaration

            // Step 2: Determine the return type of the method
            var returnConvertor = "";
            var returnPythonType = signature.GetAttr("return_annotation");

            // No specified return type (inspect._empty) is treated as object
            // Explicitly returning None is treated as void
            string returnType = TypeReflection.AnnotationAsTypename(returnPythonType);
            var returnSyntax = returnType == "NoneType" ? SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.VoidKeyword)) : TypeReflection.AsPredefinedType(returnType, out returnConvertor);

            // Step 3: Build arguments
            var parameterList = ArgumentReflection.ParameterListSyntax(signature);

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
                                        SyntaxFactory.IdentifierName("Py"),
                                        SyntaxFactory.IdentifierName("Import")),
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression,
                                                    SyntaxFactory.Literal(moduleName)))))))))));

            // Step 4: Build body, e.g.
            //using (Py.GIL())
            //{
            //    var func = mod.GetAttr("format_name");
            //    var result = func.Invoke(name.ToPython(), length.ToPython());
            //    return result.ToString()!;
            //}
            var pythonCastArguments = new List<ArgumentSyntax>();
            foreach (var parameter in parameterList.Parameters)
            {
                pythonCastArguments.Add(SyntaxFactory.Argument(SyntaxFactory.InvocationExpression(
                                                SyntaxFactory.MemberAccessExpression(
                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                    SyntaxFactory.IdentifierName(parameter.Identifier),
                                                    SyntaxFactory.IdentifierName("ToPython")))));
            }


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
                                                                SyntaxFactory.Literal(methodName))))))))))),
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
                                                    SyntaxFactory.IdentifierName("Invoke")),
                                                SyntaxFactory.ArgumentList(
                                                    SyntaxFactory.SeparatedList(pythonCastArguments)))))))),
                        SyntaxFactory.ReturnStatement(
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("result"),
                                    SyntaxFactory.IdentifierName(returnConvertor)))
                            .WithArgumentList(
                                SyntaxFactory.ArgumentList())))
                        
                    ));

            return SyntaxFactory.MethodDeclaration(
                returnSyntax,
                SyntaxFactory.Identifier(methodName.ToPascalCase()))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                    )
                .WithBody(body)
                .WithParameterList(parameterList);
        }
    }
}
