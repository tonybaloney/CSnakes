using System.Globalization;
using System.Text;

namespace CSnakes.Parser.Types;

public abstract record PythonConstant
{
    public static implicit operator PythonConstant(long value) => (Integer)value;
    public static implicit operator PythonConstant(double value) => (Float)value;
    public static implicit operator PythonConstant(string value) => (String)value;
    public static implicit operator PythonConstant(bool value) => (Bool)value;

    public sealed record Integer : PythonConstant
    {
        public enum Notation
        {
            Decimal,        // e.g. 42
            Hexadecimal,    // e.g. 0x2A
            Binary,         // e.g. 0b101010
        }

        public static Integer Decimal(long value) => new(value, Notation.Decimal);
        public static Integer Hexadecimal(long value) => new(value, Notation.Hexadecimal);
        public static Integer Binary(long value) => new(value, Notation.Binary);

        private Integer(long value, Notation notationHint)
        {
            Value = value;
            NotationHint = notationHint;
        }

        public long Value { get; init; }
        public Notation NotationHint { get; init; }

        // Equality is based on value only, not desired notation

        public bool Equals(Integer? other) => other is { Value: var v } && Value == v;
        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => NotationHint switch
        {
            Notation.Decimal => Value.ToString(),
            Notation.Hexadecimal => $"0x{Value:X}",
            Notation.Binary => $"0b{Convert.ToString(Value, 2)}",
            _ => throw new NotImplementedException()
        };

        public static implicit operator Integer(long value) => Decimal(value);
    }

    public sealed record Float : PythonConstant
    {
        public Float(double value) => Value = value;
        public double Value { get; init; }

        public override string ToString()
            => double.IsNaN(Value) ? "nan"
             : double.IsPositiveInfinity(Value) ? "inf"
             : double.IsNegativeInfinity(Value) ? "-inf"
             : Value.ToString(CultureInfo.InvariantCulture);

        public static implicit operator Float(double value) => new(value);
    }

    public sealed record String : PythonConstant
    {
        public String(string value) => Value = value;
        public string Value { get; init; }
        public override string ToString() => PythonStringFormatter.Format(Value);

        public static implicit operator String(string value) => new(value);
    }

    public sealed record ByteString : PythonConstant
    {
        private readonly string value;

        public ByteString(string value) => this.value = Checked(value);

        public string Value
        {
            get => this.value;
            init => this.value = Checked(value);
        }

        private static string Checked(string value)
        {
            // Check value doesn't contain any non-ASCII characters
            return value.All(c => c < 128)
                 ? value
                 : throw new ArgumentException("Byte strings must contain only ASCII characters.", nameof(value));
        }

        public override string ToString() => PythonStringFormatter.Format(Value, "b");
    }

    public sealed record Bool : PythonConstant
    {
        public static readonly Bool True = new(true);
        public static readonly Bool False = new(false);

        private Bool(bool value) => Value = value;

        public bool Value { get; }

        public override string ToString() => Value ? "True" : "False";

        public static implicit operator Bool(bool value) => value ? True : False;
    }

    public sealed record None : PythonConstant
    {
        public static readonly None Value = new();

        private None() { }

        public override string ToString() => "None";
    }

    public sealed record Ellipsis : PythonConstant
    {
        public static readonly Ellipsis Value = new();
        private Ellipsis() { }
        public override string ToString() => "...";
    }
}

internal static class PythonStringFormatter
{
    public static string Format(string value, string prefix = "")
    {
        var sb = new StringBuilder();
        sb.Append(prefix);
        sb.Append('\'');
        foreach (char c in value)
        {
            if (c switch { '\'' => "\\'",
                    '\\' => @"\\",
                    '\n' => "\\n",
                    '\r' => "\\r",
                    '\t' => "\\t",
                    '\b' => "\\b",
                    '\f' => "\\f",
                    '\0' => "\\0",
                    _ => null } is { } s)
            {
                sb.Append(s);
            }
            else
            {
                sb.Append(c);
            }
        }
        sb.Append('\'');
        return sb.ToString();
    }
}
