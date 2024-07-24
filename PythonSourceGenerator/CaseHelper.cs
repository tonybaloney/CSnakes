namespace PythonSourceGenerator
{
    public static class CaseHelper
    {
        public static string ToPascalCase(this string snakeCase)
        {
            return string.Join("", snakeCase.Split('_').Select(s => s.Length > 1 ? char.ToUpperInvariant(s[0]) + s.Substring(1) : "_"));
        }

        public static string ToLowerPascalCase(this string snakeCase)
        {
            // Make sure the first letter is lowercase
            return char.ToLowerInvariant(snakeCase[0]) + ToPascalCase(snakeCase).Substring(1);
        }
    }
}
