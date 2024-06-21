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
            if (pythonType.Contains("[") && pythonType.Contains("]"))
            {
                var genericName = pythonType.Split('[')[0];
                // Get last occurence of ] in pythonType
                var genericOf = pythonType.Substring(pythonType.IndexOf('[') + 1, pythonType.LastIndexOf(']') - pythonType.IndexOf('[') - 1); 
                switch (genericName)
                {
                    case "list":
                        var innerTypeSyntax = AsPredefinedType(genericOf.Trim(), out _);
                        convertor = "AsList<" + innerTypeSyntax.ToString() + ">";
                        return SyntaxFactory.GenericName(
                            SyntaxFactory.Identifier("List"))
                            .WithTypeArgumentList(
                                SyntaxFactory.TypeArgumentList(
                                    SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
                                        innerTypeSyntax)));
                    case "tuple":
                        var tupleTypes = genericOf.Split(',');
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
