using CSnakes.Parser.Types;
using System.Globalization;

namespace CSnakes.Tests;
public class PythonConstantTests
{
    public sealed class BinaryIntegerTests
    {
        [Theory]
        [InlineData(0, "0b0")]
        [InlineData(1, "0b1")]
        [InlineData(2, "0b10")]
        [InlineData(3, "0b11")]
        [InlineData(42, "0b101010")]
        public void ToString_Returns_BinaryFormatting(long input, string expected)
        {
            var n = new PythonConstant.BinaryInteger(input);
            Assert.Equal(expected, n.ToString());
        }
    }

    public sealed class FloatTests
    {
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
    }
}
