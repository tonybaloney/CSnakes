using System.Globalization;
using static CSnakes.Parser.Types.PythonConstant.String;

namespace CSnakes.Parser.Types;

public abstract class PythonConstant
{
    public abstract override string ToString();

    public class Integer(long value) : PythonConstant
    {
        public long Value { get; } = value;
        public override string ToString() => Value.ToString();
    }

    public sealed class HexadecimalInteger(long value) : Integer(value)
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
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }

    public sealed class String(string value) : PythonConstant
    {
        public string Value { get; } = value;
        public override string ToString() => Value;
    }

    public sealed class ByteString : PythonConstant
    {
        private readonly string value;
        public ByteString(string value)
        {
            // Check value doesn't contain any non-ASCII characters
            if (value.Any(c => c > 127))
                throw new ArgumentException("Byte strings must contain only ASCII characters.", nameof(value));
            this.value = value;
        }
        public string Value => value;
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

    public sealed class Ellipsis : PythonConstant
    {
        public static readonly Ellipsis Value = new();
        private Ellipsis() { }
        public override string ToString() => "...";
    }
}
