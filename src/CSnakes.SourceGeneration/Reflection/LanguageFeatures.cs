using Microsoft.CodeAnalysis.CSharp;

namespace CSnakes.Reflection;

[Flags]
public enum LanguageFeatures
{
    None = 0,
    /// <summary>
    /// <see href="https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#params-collections"><c>params</c> collections</see>.
    /// </summary>
    ParamsCollections = 1,
}

public static class LanguageVersionExtensions
{
    extension(LanguageVersion version)
    {
        public LanguageFeatures Features =>
            version > LanguageVersion.CSharp12
            ? LanguageFeatures.ParamsCollections // available in C# 13+
            : LanguageFeatures.None;
    }
}

public static class LanguageFeaturesExtensions
{
    public static bool Has(this LanguageFeatures actual, LanguageFeatures expected) =>
        (actual & expected) == expected;
}
