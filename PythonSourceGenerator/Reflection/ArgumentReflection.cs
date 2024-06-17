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
            var pythonParameters = signature.GetAttr("parameters").As<PyDict>();
            foreach (var pythonParameter in pythonParameters.Items())
            {
                var name = pythonParameters[0].GetAttr("name").ToString();
                var type = pythonParameters[1].GetAttr("annotation").ToString();
                var convertor = "";
                parameterListSyntax.Add(ArgumentSyntax(name, type, out convertor));
            }
            return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameterListSyntax));
        }
    }
}
