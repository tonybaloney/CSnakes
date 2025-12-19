using CSnakes.Parser.Types;
using System.Globalization;

namespace CSnakes.Tests;
public class PythonConstantTests
{
    public class IntegerTests
    {
        [Fact]
        public void Long_To_PythonConstant_Creates_Integer()
        {
            PythonConstant constant = 42L;
            var integer = Assert.IsType<PythonConstant.Integer>(constant);
            Assert.Equal(42L, integer.Value);
        }

        [Fact]
        public void Instances_With_Same_Value_Are_Equal()
        {
            PythonConstant.Integer int1 = 42;
            PythonConstant.Integer int2 = 42;

            Assert.Equal(int1, int2);
            Assert.True(int1 == int2);
            Assert.False(int1 != int2);
            Assert.Equal(int1.GetHashCode(), int2.GetHashCode());
        }

        [Fact]
        public void Instances_With_Different_Values_Are_Not_Equal()
        {
            PythonConstant.Integer int1 = 42;
            PythonConstant.Integer int2 = 24;

            Assert.NotEqual(int1, int2);
            Assert.False(int1 == int2);
            Assert.True(int1 != int2);
        }

        [Fact]
        public void Supports_Value_Init()
        {
            var integer = PythonConstant.Integer.Decimal(10) with { Value = 42 };
            Assert.Equal(42, integer.Value);
        }

        [Fact]
        public void ToString_Returns_Value_As_String()
        {
            PythonConstant.Integer integer = 42;
            Assert.Equal("42", integer.ToString());
        }

        [Fact]
        public void HexadecimalInteger_With_Same_Value_Are_Equal()
        {
            var hex1 = PythonConstant.Integer.Hexadecimal(255);
            var hex2 = PythonConstant.Integer.Hexadecimal(255);

            Assert.Equal(hex1, hex2);
            Assert.True(hex1 == hex2);
            Assert.Equal(hex1.GetHashCode(), hex2.GetHashCode());
        }

        [Fact]
        public void HexadecimalInteger_ToString_Returns_Hex_Format()
        {
            var hex = PythonConstant.Integer.Hexadecimal(255);
            Assert.Equal("0xFF", hex.ToString());
        }

        [Fact]
        public void BinaryInteger_With_Same_Value_Are_Equal()
        {
            var bin1 = PythonConstant.Integer.Binary(15);
            var bin2 = PythonConstant.Integer.Binary(15);

            Assert.Equal(bin1, bin2);
            Assert.True(bin1 == bin2);
            Assert.Equal(bin1.GetHashCode(), bin2.GetHashCode());
        }

        [Theory]
        [InlineData(0, "0b0")]
        [InlineData(1, "0b1")]
        [InlineData(2, "0b10")]
        [InlineData(3, "0b11")]
        [InlineData(42, "0b101010")]
        public void BinaryInteger_ToString_Returns_Binary_Format(int value, string expected)
        {
            var bin = PythonConstant.Integer.Binary(value);
            Assert.Equal(expected, bin.ToString());
        }
    }

    public class FloatTests
    {
        [Fact]
        public void Double_To_PythonConstant_Creates_Float()
        {
            PythonConstant constant = 3.14;
            var floatConstant = Assert.IsType<PythonConstant.Float>(constant);
            Assert.Equal(3.14, floatConstant.Value);
        }

        [Fact]
        public void Instances_With_Same_Value_Are_Equal()
        {
            var float1 = new PythonConstant.Float(3.14);
            var float2 = new PythonConstant.Float(3.14);

            Assert.Equal(float1, float2);
            Assert.True(float1 == float2);
            Assert.Equal(float1.GetHashCode(), float2.GetHashCode());
        }

        [Fact]
        public void Supports_Value_Init()
        {
            var floatConstant = new PythonConstant.Float(1.0) { Value = 3.14 };
            Assert.Equal(3.14, floatConstant.Value);
        }

        [Fact]
        public void ToString_Is_Not_Culture_Specific()
        {
            var testCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            testCulture.NumberFormat.NumberDecimalSeparator = ",";
            testCulture.NumberFormat.NumberGroupSeparator = ".";

            var oldCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.ReadOnly(testCulture);

            try
            {
                var n = new PythonConstant.Float(1_737.4);
                Assert.Equal("1737.4", n.ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = oldCulture;
            }
        }

        [Theory]
        [InlineData(0.0, "0.0")]
        [InlineData(0.5, "0.5")]
        [InlineData(1.5, "1.5")]
        [InlineData(3.141592653589793, "3.141592653589793")]
        [InlineData(-42.125, "-42.125")]
        [InlineData(float.PositiveInfinity, "inf")]
        [InlineData(float.NegativeInfinity, "-inf")]
        [InlineData(float.NaN, "nan")]
        //
        // The following test cases are taken from CPython:
        // https://github.com/python/cpython/blob/df793163d5821791d4e7caf88885a2c11a107986/Lib/test/test_float.py#L865-L897
        //
        // output always includes *either* a decimal point and at
        // least one digit after that point, or an exponent.
        [InlineData(1.0, "1.0")]
        [InlineData(0.01, "0.01")]
        [InlineData(0.02, "0.02")]
        [InlineData(0.03, "0.03")]
        [InlineData(0.04, "0.04")]
        [InlineData(0.05, "0.05")]
        [InlineData(1.23456789, "1.23456789")]
        [InlineData(10.0, "10.0")]
        [InlineData(100.0, "100.0")]
        // values >= 1e16 get an exponent...
        [InlineData(1000000000000000.0, "1000000000000000.0")]
        [InlineData(9999999999999990.0, "9999999999999990.0")]
        [InlineData(1e+16, "1e+16")]
        [InlineData(1e+17, "1e+17")]
        // ... and so do values < 1e-4
        [InlineData(0.001, "0.001")]
        [InlineData(0.001001, "0.001001")]
        [InlineData(0.00010000000000001, "0.00010000000000001")]
        [InlineData(0.0001, "0.0001")]
        [InlineData(9.999999999999e-05, "9.999999999999e-05")]
        [InlineData(1e-05, "1e-05")]
        // values designed to provoke failure if the FPU rounding
        // precision isn't set correctly
        [InlineData(8.72293771110361e+25, "8.72293771110361e+25")]
        [InlineData(7.47005307342313e+26, "7.47005307342313e+26")]
        [InlineData(2.86438000439698e+28, "2.86438000439698e+28")]
        [InlineData(8.89142905246179e+28, "8.89142905246179e+28")]
        [InlineData(3.08578087079232e+35, "3.08578087079232e+35")]
        public void ToString_Formats_Correctly(double value, string expected)
        {
            var floatConstant = new PythonConstant.Float(value);
            Assert.Equal(expected, floatConstant.ToString());
        }

        [Fact]
        public void ToString_Formats_NaN()
        {
            var floatConstant = new PythonConstant.Float(double.NaN);
            Assert.Equal("nan", floatConstant.ToString());
        }

        [Fact]
        public void ToString_Formats_PositiveInfinity()
        {
            var floatConstant = new PythonConstant.Float(double.PositiveInfinity);
            Assert.Equal("inf", floatConstant.ToString());
        }

        [Fact]
        public void ToString_Formats_NegativeInfinity()
        {
            var floatConstant = new PythonConstant.Float(double.NegativeInfinity);
            Assert.Equal("-inf", floatConstant.ToString());
        }
    }

    public class StringTests
    {
        [Fact]
        public void String_To_PythonConstant_Creates_String()
        {
            PythonConstant constant = "hello";
            var stringConstant = Assert.IsType<PythonConstant.String>(constant);
            Assert.Equal("hello", stringConstant.Value);
        }

        [Fact]
        public void Instances_With_Same_Value_Are_Equal()
        {
            var str1 = new PythonConstant.String("hello");
            var str2 = new PythonConstant.String("hello");

            Assert.Equal(str1, str2);
            Assert.True(str1 == str2);
            Assert.Equal(str1.GetHashCode(), str2.GetHashCode());
        }

        [Fact]
        public void Instances_With_Different_Values_Are_Not_Equal()
        {
            var str1 = new PythonConstant.String("hello");
            var str2 = new PythonConstant.String("world");

            Assert.NotEqual(str1, str2);
            Assert.False(str1 == str2);
            Assert.True(str1 != str2);
        }

        [Fact]
        public void Supports_Value_Init()
        {
            var stringConstant = new PythonConstant.String("old") { Value = "new" };
            Assert.Equal("new", stringConstant.Value);
        }

        [Theory]
        [InlineData("hello", "'hello'")]
        [InlineData("", "''")]
        [InlineData("it's working", @"'it\'s working'")]
        [InlineData("path\\to\\file", @"'path\\to\\file'")]
        [InlineData("line1\nline2", @"'line1\nline2'")]
        [InlineData("tab\there", @"'tab\there'")]
        [InlineData("carriage\rreturn", @"'carriage\rreturn'")]
        [InlineData("back\bspace", @"'back\bspace'")]
        [InlineData("form\ffeed", @"'form\ffeed'")]
        [InlineData("null\0char", @"'null\0char'")]
        [InlineData("café", "'café'")]
        [InlineData("Hello 世界", "'Hello 世界'")]
        public void ToString_Formats_As_Python_Literal(string value, string expected)
        {
            var stringConstant = new PythonConstant.String(value);
            Assert.Equal(expected, stringConstant.ToString());
        }

        [Fact]
        public void ToString_Handles_Multiple_Escape_Sequences()
        {
            var stringConstant = new PythonConstant.String("line1\nline2\ttab");
            Assert.Equal(@"'line1\nline2\ttab'", stringConstant.ToString());
        }

        [Fact]
        public void ToString_Handles_Mixed_Escapes()
        {
            var stringConstant = new PythonConstant.String("quote:' backslash:\\ newline:\n");
            Assert.Equal(@"'quote:\' backslash:\\ newline:\n'", stringConstant.ToString());
        }

        [Fact]
        public void ToString_Handles_Consecutive_Backslashes()
        {
            var stringConstant = new PythonConstant.String("\\\\\\");
            Assert.Equal(@"'\\\\\\'", stringConstant.ToString());
        }

        [Theory]
        [InlineData("\n", @"'\n'")]
        [InlineData("\r", @"'\r'")]
        [InlineData("\t", @"'\t'")]
        [InlineData("\b", @"'\b'")]
        [InlineData("\f", @"'\f'")]
        [InlineData("\0", @"'\0'")]
        [InlineData("\\", @"'\\'")]
        [InlineData("'", @"'\''")]
        public void ToString_Escapes_Special_Characters(string value, string expected)
        {
            var stringConstant = new PythonConstant.String(value);
            Assert.Equal(expected, stringConstant.ToString());
        }
    }

    public class BoolTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Bool_To_PythonConstant_Creates_Bool(bool value)
        {
            PythonConstant constant = value;
            var boolConstant = Assert.IsType<PythonConstant.Bool>(constant);
            Assert.Equal(value, boolConstant.Value);

            // Verify singleton pattern is maintained
            Assert.Same(value ? PythonConstant.Bool.True : PythonConstant.Bool.False, boolConstant);
        }

        [Fact]
        public void True_Is_Singleton()
        {
            var bool1 = PythonConstant.Bool.True;
            var bool2 = PythonConstant.Bool.True;
            Assert.Same(bool1, bool2);
        }

        [Fact]
        public void False_Is_Singleton()
        {
            var bool1 = PythonConstant.Bool.False;
            var bool2 = PythonConstant.Bool.False;
            Assert.Same(bool1, bool2);
        }

        [Fact]
        public void True_ToString_Returns_True()
        {
            Assert.Equal("True", PythonConstant.Bool.True.ToString());
        }

        [Fact]
        public void False_ToString_Returns_False()
        {
            Assert.Equal("False", PythonConstant.Bool.False.ToString());
        }
    }

    public class ByteStringTests
    {
        [Fact]
        public void Instances_With_Same_Value_Are_Equal()
        {
            var bytes1 = new PythonConstant.ByteString("test");
            var bytes2 = new PythonConstant.ByteString("test");

            Assert.Equal(bytes1, bytes2);
            Assert.True(bytes1 == bytes2);
            Assert.Equal(bytes1.GetHashCode(), bytes2.GetHashCode());
        }

        [Fact]
        public void Supports_Value_Init()
        {
            var byteString = new PythonConstant.ByteString("old") { Value = "new" };
            Assert.Equal("new", byteString.Value);
        }

        [Fact]
        public void Constructor_Accepts_ASCII_Characters()
        {
            var chars = Enumerable.Range(0, 128).Select(i => (char)i).ToArray();
            var str = new string(chars);
            var byteString = new PythonConstant.ByteString(str);
            Assert.Equal(str, byteString.Value);
        }

        public static TheoryData<string> InvalidValues = new()
        {
            "Héllo", // é is not ASCII
            "Hello£", // £ is not ASCII
            "こんにちは", // Japanese characters
            "🚀", // Emoji
            "\x80", // Character 128
        };

        [Theory]
        [MemberData(nameof(InvalidValues))]
        public void Constructor_Throws_For_NonASCII_Characters(string invalidValue)
        {
            void Act() => _ = new PythonConstant.ByteString(invalidValue);
            var exception = Assert.Throws<ArgumentException>(Act);
            Assert.Equal("Byte strings must contain only ASCII characters. (Parameter 'value')", exception.Message);
        }

        [Theory]
        [MemberData(nameof(InvalidValues))]
        public void Value_Init_Validates_ASCII(string invalidValue)
        {
            void Act() => _ = new PythonConstant.ByteString("valid") { Value = invalidValue };
            var exception = Assert.Throws<ArgumentException>(Act);
            Assert.Equal("Byte strings must contain only ASCII characters. (Parameter 'value')", exception.Message);
        }

        [Fact]
        public void ToString_Returns_Value()
        {
            var byteString = new PythonConstant.ByteString("test");
            Assert.Equal("b'test'", byteString.ToString());
        }

        [Theory]
        [InlineData("hello", "b'hello'")]
        [InlineData("", "b''")]
        [InlineData("it's working", @"b'it\'s working'")]
        [InlineData("path\\to\\file", @"b'path\\to\\file'")]
        [InlineData("line1\nline2", @"b'line1\nline2'")]
        [InlineData("tab\there", @"b'tab\there'")]
        [InlineData("carriage\rreturn", @"b'carriage\rreturn'")]
        [InlineData("back\bspace", @"b'back\bspace'")]
        [InlineData("form\ffeed", @"b'form\ffeed'")]
        [InlineData("null\0char", @"b'null\0char'")]
        public void ToString_Formats_As_Python_Literal(string value, string expected)
        {
            var byteString = new PythonConstant.ByteString(value);
            Assert.Equal(expected, byteString.ToString());
        }

        [Fact]
        public void ToString_Handles_Multiple_Escape_Sequences()
        {
            var byteString = new PythonConstant.ByteString("line1\nline2\ttab");
            Assert.Equal(@"b'line1\nline2\ttab'", byteString.ToString());
        }

        [Fact]
        public void ToString_Handles_Mixed_Escapes()
        {
            var byteString = new PythonConstant.ByteString("quote:' backslash:\\ newline:\n");
            Assert.Equal(@"b'quote:\' backslash:\\ newline:\n'", byteString.ToString());
        }

        [Fact]
        public void ToString_Handles_Consecutive_Backslashes()
        {
            var byteString = new PythonConstant.ByteString("\\\\\\");
            Assert.Equal(@"b'\\\\\\'", byteString.ToString());
        }

        [Theory]
        [InlineData("\n", @"b'\n'")]
        [InlineData("\r", @"b'\r'")]
        [InlineData("\t", @"b'\t'")]
        [InlineData("\b", @"b'\b'")]
        [InlineData("\f", @"b'\f'")]
        [InlineData("\0", @"b'\0'")]
        [InlineData("\\", @"b'\\'")]
        [InlineData("'", @"b'\''")]
        public void ToString_Escapes_Special_Characters(string value, string expected)
        {
            var byteString = new PythonConstant.ByteString(value);
            Assert.Equal(expected, byteString.ToString());
        }
    }

    public class NoneTests
    {
        [Fact]
        public void Is_Singleton()
        {
            var none1 = PythonConstant.None.Value;
            var none2 = PythonConstant.None.Value;
            Assert.Same(none1, none2);
        }

        [Fact]
        public void ToString_Returns_None()
        {
            Assert.Equal("None", PythonConstant.None.Value.ToString());
        }
    }

    public class EllipsisTests
    {
        [Fact]
        public void Is_Singleton()
        {
            var ellipsis1 = PythonConstant.Ellipsis.Value;
            var ellipsis2 = PythonConstant.Ellipsis.Value;
            Assert.Same(ellipsis1, ellipsis2);
        }

        [Fact]
        public void ToString_Returns_Ellipsis()
        {
            Assert.Equal("...", PythonConstant.Ellipsis.Value.ToString());
        }
    }
}
