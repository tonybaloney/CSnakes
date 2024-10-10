using CSnakes.Parser.Types;
using System.Globalization;

namespace CSnakes.Tests;
public class PythonConstantTests
{
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
                var n = new PythonConstant.Float(3.14);
                Assert.Equal("3.14", n.ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture= oldCulture;
            }
        }
    }
}
