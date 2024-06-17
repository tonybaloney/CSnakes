using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Python.Runtime;
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
            var pythonParameters = signature.GetAttr("parameters");
            for (int i = 0; i < pythonParameters.Length(); i++)
            {
                var name = pythonParameters[i].GetAttr("name").ToString();
                var type = pythonParameters[i].GetAttr("annotation").ToString();
                var convertor = "";
                parameterListSyntax.Add(ArgumentSyntax(name, type, out convertor));
            }
            return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameterListSyntax));
        }
    }
}
