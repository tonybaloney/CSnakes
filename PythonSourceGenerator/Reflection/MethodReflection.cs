using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Python.Runtime;

namespace PythonSourceGenerator.Reflection
{
    public static class MethodReflection
    {
        public static MethodDeclarationSyntax FromMethod(PyObject signature, string name)
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
            return SyntaxFactory.MethodDeclaration(
                returnSyntax,
                SyntaxFactory.Identifier(name.ToPascalCase()))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                    )
                .WithBody(
                    SyntaxFactory.Block())
                .WithParameterList(parameterList);
        }
    }
}
