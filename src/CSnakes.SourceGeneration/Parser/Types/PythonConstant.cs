namespace CSnakes.Parser.Types;

public abstract class PythonConstant
{
    public abstract override string ToString();

    public class Integer(long value) : PythonConstant
    {
        public long Value { get; } = value;
        public override string ToString() => Value.ToString();
    }

    public sealed class HexidecimalInteger(long value) : Integer(value)
    {
        public override string ToString() => $"0x{Value:X}";
    }

    public sealed class BinaryInteger(long value) : Integer(value)
    {
        public override string ToString() => $"0b{Value:X}";
    }

    public sealed class Float(double value) : PythonConstant
    {
        public double Value { get; } = value;
        public override string ToString() => Value.ToString();
    }

    public sealed class String(string value) : PythonConstant
    {
        public string Value { get; } = value;
        public override string ToString() => Value;
    }

    public sealed class Bool : PythonConstant
    {
        public static readonly Bool True = new(true);
        public static readonly Bool False = new(false);

        private Bool(bool value) => Value = value;

        public bool Value { get; }

        public override string ToString() => Value.ToString();
    }

    public sealed class None : PythonConstant
    {
        public static readonly None Value = new();

        private None() { }

        public override string ToString() => "None";
    }
}
