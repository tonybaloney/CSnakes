namespace PythonSourceGenerator.Parser.Types;

public class PythonConstant
{
    public bool IsInteger { get; set; }
    public int IntegerValue { get; set; }

    public bool IsString { get; set; }
    public string? StringValue { get; set; }

    public bool IsNone { get; set; }

    public bool IsFloat { get; set; }
    public double FloatValue { get; set; }

    public bool IsBool { get; set; }
    public bool BoolValue { get; set; }

    public override string ToString()
    {
        if (IsInteger)
        {
            return IntegerValue.ToString();
        }
        if (IsString)
        {
            return StringValue ?? throw new ArgumentNullException(nameof(StringValue));
        }
        if (IsFloat)
        {
            return FloatValue.ToString();
        }
        if (IsNone)
        {
            return "None";
        }
        if (IsBool)
        {
            return BoolValue.ToString();
        }
        return "unknown";
    }
}
