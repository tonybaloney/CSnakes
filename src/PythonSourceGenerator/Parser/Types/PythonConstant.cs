namespace PythonSourceGenerator.Parser.Types;

public class PythonConstant
{
    public enum ConstantType
    {
        Integer,
        HexidecimalInteger,
        BinaryInteger,
        Float,
        String,
        Bool,
        None,
    }

    public PythonConstant() { }

    public PythonConstant(long value)
    {
        Type = ConstantType.Integer;
        IntegerValue = value;
    }

    public PythonConstant(double value)
    {
        Type = ConstantType.Float;
        FloatValue = value;
    }

    public PythonConstant(string value)
    {
        Type = ConstantType.String;
        StringValue = value;
    }

    public PythonConstant(bool value)
    {
        Type = ConstantType.Bool;
        BoolValue = value;
    }

    public ConstantType Type { get; set; }

    public long IntegerValue { get; set; }

    public string? StringValue { get; set; }

    public double FloatValue { get; set; }

    public bool BoolValue { get; set; }

    public bool IsInteger { get => Type == ConstantType.Integer || Type == ConstantType.BinaryInteger || Type == ConstantType.HexidecimalInteger; }

    public override string ToString()
    {
        switch (Type)
        {
            case ConstantType.Integer:
                return IntegerValue.ToString();
            case ConstantType.HexidecimalInteger:
                return $"0x{IntegerValue:X}";
            case ConstantType.BinaryInteger:
                return $"0b{IntegerValue:X}";
            case ConstantType.Float:
                return FloatValue.ToString();
            case ConstantType.Bool:
                return BoolValue.ToString();
            case ConstantType.String:
                return StringValue ?? throw new ArgumentNullException(nameof(StringValue));
            case ConstantType.None:
                return "None";
            default:
                return "unknown";
        }
    }
}
