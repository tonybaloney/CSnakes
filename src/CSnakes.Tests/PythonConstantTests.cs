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
        [InlineData(0.0, "0")]
        [InlineData(1.5, "1.5")]
        [InlineData(-42.125, "-42.125")]
        public void ToString_Formats_Correctly(double value, string expected)
        {
            var floatConstant = new PythonConstant.Float(value);
            Assert.Equal(expected, floatConstant.ToString());
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
            Assert.Equal("test", byteString.ToString());
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
