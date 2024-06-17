using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
    }
}
