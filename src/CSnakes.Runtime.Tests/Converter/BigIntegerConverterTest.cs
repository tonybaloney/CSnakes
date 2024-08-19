using CSnakes.Runtime.Python;
using System.ComponentModel;
using System.Numerics;

namespace CSnakes.Runtime.Tests.Converter;

public class BigIntegerConverterTest : RuntimeTestBase
{
    [Fact]
    public void TestVeryBigNumbers()
    {
        const string number = "12345678987654345678764345678987654345678765";
        TypeConverter td = TypeDescriptor.GetConverter(typeof(PyObject));
        // Something that is too big for a long (I8)
        BigInteger input = BigInteger.Parse(number);

        Assert.True(td.CanConvertFrom(typeof(BigInteger)));

        using (GIL.Acquire())
        {
            using PyObject? pyObj = td.ConvertFrom(input) as PyObject;

            Assert.NotNull(pyObj);
            Assert.Equal(number, pyObj!.ToString());

            Assert.True(td.CanConvertTo(typeof(BigInteger)));

            // Convert back
            BigInteger integer = (BigInteger) td.ConvertTo(pyObj, typeof(BigInteger))!;
            Assert.Equal(input, integer);
        }
    }
}
