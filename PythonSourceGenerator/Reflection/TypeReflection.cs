using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Python.Runtime;
using System;

namespace PythonSourceGenerator.Reflection
{
    public static class TypeReflection
    {
        public static PredefinedTypeSyntax AsPredefinedType(string pythonType, out string convertor)
        {
            switch (pythonType)
            {
                case "int":
                    convertor = "ToInt64";
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.LongKeyword));
                case "str":
                    convertor = "ToString";
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.StringKeyword));
                case "float":
                    convertor = "ToDouble";
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.DoubleKeyword));
                case "bool":
                    convertor = "ToBoolean";
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.BoolKeyword));
                // Todo more types...
                default:
                    convertor = "AsManagedObject";
                    return SyntaxFactory.PredefinedType(
                        SyntaxFactory.Token(SyntaxKind.ObjectKeyword)); // TODO : Should be nullable
            }
        }

        internal static string AnnotationAsTypename(PyObject pyObject)
        {
            // If pyObject is a class object or type object
            if (pyObject.HasAttr("__name__"))
            {
                return pyObject.GetAttr("__name__").ToString();
            }
            // If pyObject is a string
            if (PyString.IsStringType(pyObject))
            {
                return pyObject.ToString();
            }
            // If pyObject is a type
            if (pyObject.HasAttr("__class__"))
            {
                return pyObject.GetAttr("__class__").GetAttr("__name__").ToString();
            }
            // Just return the string representation of the object
            return pyObject.ToString();
        }
    }
}
