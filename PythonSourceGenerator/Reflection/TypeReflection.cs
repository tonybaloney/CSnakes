using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Python.Runtime;

namespace PythonSourceGenerator.Reflection
{
    public static class TypeReflection
    {
        public static TypeSyntax AsPredefinedType(string pythonType, out string convertor)
        {
            // If type is an alias, e.g. "list[int]", "list[float]", etc.
            if (pythonType.Contains("["))
            {
                var alias = pythonType.Split('[');
                var innerType = alias[1].Replace("]", "");
                var innerTypeSyntax = AsPredefinedType(innerType, out _);
                switch (alias[0])
                {
                    case "list":
                        convertor = "AsList<" + innerTypeSyntax.ToString() + ">";
                        return SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier("List"))
                            .WithTypeArgumentList(
                                SyntaxFactory.TypeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                        innerTypeSyntax)));
                    case "tuple":
                        var tupleTypes = innerType.Split(',');
                        var tupleTypeSyntax = new TypeSyntax[tupleTypes.Length];
                        if (tupleTypes.Length > 8) // TODO: Implement up to 21
                        {
                            break; // Not supported.
                        }
                        for (int i = 0; i < tupleTypes.Length; i++)
                        {
                            tupleTypeSyntax[i] = AsPredefinedType(tupleTypes[i].Trim(), out _);
                        }
                        convertor = "AsTuple<" + string.Join<TypeSyntax>(", ", tupleTypeSyntax) + ">";
                        return SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier("Tuple"))
                            .WithTypeArgumentList(
                                SyntaxFactory.TypeArgumentList(
                                    SyntaxFactory.SeparatedList<TypeSyntax>(
                                        tupleTypeSyntax)));
                    // Todo more types... see https://docs.python.org/3/library/stdtypes.html#standard-generic-classes
                    default:
                        convertor = null;
                        return SyntaxFactory.ParseTypeName("PyObject"); // TODO : Should be nullable?
                }
            }
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
                    convertor = null;
                    return SyntaxFactory.ParseTypeName("PyObject");  // TODO : Should be nullable?
            }
        }

        internal static string AnnotationAsTypename(PyObject pyObject)
        {
            var typeName = pyObject.GetPythonType().Name;
            // Is class?
            switch (typeName)
            {
                case "type":
                case "class":
                    return pyObject.GetAttr("__name__").ToString();
                default:
                    return pyObject.Repr().ToString();
            }
            
        }
    }
}
