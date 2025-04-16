namespace CSnakes;
public abstract record TypeInfo
{
    public static readonly TypeInfo Boolean = new BooleanTypeInfo();
    public static readonly TypeInfo Int64 = new Int64TypeInfo();
    public static readonly TypeInfo Double = new DoubleTypeInfo();
    public static readonly TypeInfo String = new StringTypeInfo();
    public static readonly TypeInfo ByteArray = new ByteArrayTypeInfo();

    public abstract string GetReturnTypeSyntax();
    public abstract string GetImporterTypeSyntax();

    private abstract record StaticTypeInfo(string ReturnType, string ImporterType) : TypeInfo
    {
        public override string GetReturnTypeSyntax() => ReturnType;
        public override string GetImporterTypeSyntax() => ImporterType;
    }

    private const string PyObjectImporters = "global::CSnakes.Runtime.Python.PyObjectImporters";

    private sealed record BooleanTypeInfo() : StaticTypeInfo("bool", $"{PyObjectImporters}.Boolean");
    private sealed record Int64TypeInfo() : StaticTypeInfo("long", $"{PyObjectImporters}.Int64");
    private sealed record DoubleTypeInfo() : StaticTypeInfo("double", $"{PyObjectImporters}.Double");
    private sealed record StringTypeInfo() : StaticTypeInfo("string", $"{PyObjectImporters}.String");
    private sealed record ByteArrayTypeInfo() : StaticTypeInfo("byte[]", $"{PyObjectImporters}.ByteArray");
}

public sealed record ListTypeInfo(TypeInfo ItemType) : TypeInfo
{
    public override string GetReturnTypeSyntax() =>
        $"global::System.Collections.Generic.IReadOnlyList<{ItemType.GetReturnTypeSyntax()}>";

    public override string GetImporterTypeSyntax() =>
        $"global::CSnakes.Runtime.Python.PyObjectImporters.List<{ItemType.GetReturnTypeSyntax()}, {ItemType.GetImporterTypeSyntax()}>";
}

public sealed record DictionaryTypeInfo(TypeInfo KeyType, TypeInfo ValueType) : TypeInfo
{
    public override string GetReturnTypeSyntax() =>
        $"global::System.Collections.Generic.IReadOnlyDictionary<{KeyType.GetReturnTypeSyntax()}, {ValueType.GetReturnTypeSyntax()}>";

    public override string GetImporterTypeSyntax() =>
        $"global::CSnakes.Runtime.Python.PyObjectImporters.Dictionary<{KeyType.GetReturnTypeSyntax()}, {ValueType.GetReturnTypeSyntax()}, {KeyType.GetImporterTypeSyntax()}, {ValueType.GetImporterTypeSyntax()}>";
}
