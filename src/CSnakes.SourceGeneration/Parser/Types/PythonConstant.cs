using System.Runtime.InteropServices;
using System.Globalization;

namespace CSnakes.Parser.Types;

public readonly struct PythonConstant
{
    public enum ConstantType
    {
        None,
        Integer,
        HexidecimalInteger,
        BinaryInteger,
        Float,
        String,
        Bool,
    }

    readonly Union union;   // union storage for value primitives (bool, long & double)
    readonly string? str;   // storage for string reference

    private PythonConstant(ConstantType type, string str) :
        this(type, default, str) { }

    private PythonConstant(ConstantType type, Union union, string? str = null) =>
        (Type, this.union, this.str) = (type, union, str);

    public sealed class Float(double value) : PythonConstant
    {
        public double Value { get; } = value;
        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Represents a union of all the primitive types supported by <see cref="PythonConstant"/>.
    /// This structure occupies the memory of the largest field as opposed to the sum of all fields
    /// since they are all physically stored at the beginning of the structure and permit to
    /// optimise space.
    /// </summary>

    [StructLayout(LayoutKind.Explicit)]
    private readonly struct Union
    {
        [FieldOffset(0)] public readonly bool Boolean;
        [FieldOffset(0)] public readonly long Int64;
        [FieldOffset(0)] public readonly double Double;

        public Union(bool value)   => this.Boolean = value;
        public Union(long value)   => this.Int64 = value;
        public Union(double value) => this.Double = value;
    }

    public PythonConstant()             : this(ConstantType.None, default(Union)) { }
    public PythonConstant(long value)   : this(ConstantType.Integer, new Union(value)) { }
    public PythonConstant(double value) : this(ConstantType.Float, new Union(value)) { }
    public PythonConstant(string value) : this(ConstantType.String, value) { }
    public PythonConstant(bool value)   : this(ConstantType.Bool, new Union(value)) { }

    public static readonly PythonConstant None = new();

    public static PythonConstant HexadecimalInteger(long value) =>
        new(ConstantType.HexidecimalInteger, new Union(value));

    public static PythonConstant BinaryInteger(long value) =>
        new(ConstantType.BinaryInteger, new Union(value));

    public ConstantType Type { get; }

    public long?   BinaryIntegerValue      => Type is ConstantType.BinaryInteger ? this.union.Int64 : null;
    public long?   HexidecimalIntegerValue => Type is ConstantType.HexidecimalInteger ? this.union.Int64 : null;
    public long?   IntegerValue            => IsInteger ? this.union.Int64 : null;
    public string? StringValue             => Type is ConstantType.String ? this.str : null;
    public double? FloatValue              => Type is ConstantType.Float ? this.union.Double : null;
    public bool?   BoolValue               => Type is ConstantType.Bool ? this.union.Boolean : null;

    public bool IsNone    => Type is ConstantType.None;
    public bool IsInteger => Type is ConstantType.Integer or ConstantType.BinaryInteger or ConstantType.HexidecimalInteger;

    public override string ToString() => Type switch
    {
        ConstantType.Integer => IntegerValue.ToString(),
        ConstantType.HexidecimalInteger => $"0x{IntegerValue:X}",
        ConstantType.BinaryInteger => $"0b{IntegerValue:X}",
        ConstantType.Float => FloatValue.ToString(),
        ConstantType.Bool => BoolValue.ToString(),
        ConstantType.String => StringValue ?? throw new ArgumentNullException(nameof(StringValue)),
        ConstantType.None => "None",
        _ => "unknown"
    };
}
