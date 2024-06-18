using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Python.Runtime;
using PythonSourceGenerator.Types;
using System.Collections.Generic;

namespace PythonSourceGenerator.Reflection
{
    public class ArgumentReflection
    {
        public static ParameterSyntax ArgumentSyntax(string name, string type, out string convertor)
        {
            return SyntaxFactory.Parameter(SyntaxFactory.Identifier(name.ToPascalCase()))
                .WithType(TypeReflection.AsPredefinedType(type, out convertor));
        }

        public static ParameterListSyntax ParameterListSyntax(PyObject signature)
        {
            var parameterListSyntax = new List<ParameterSyntax>();
            var parameters = signature.GetAttr("parameters");
            foreach (var pythonParameter in parameters.Items())
            {
                var name = pythonParameter[0].ToString();
                var type = TypeReflection.AnnotationAsTypename(pythonParameter[1].GetAttr("annotation"));
                var convertor = "";
                parameterListSyntax.Add(ArgumentSyntax(name, type, out convertor));
            }
            return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameterListSyntax));
        }
    }
}
