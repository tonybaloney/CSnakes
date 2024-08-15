using CSnakes.Runtime.Python;
using System.ComponentModel;

namespace CSnakes.Runtime.Tests.Converter;

public class BytesConverterTest : RuntimeTestBase
{
    [Theory]
    [InlineData(new byte[]{ 0x01, 0x02, 0x03, 0x04 })]
    [InlineData(new byte[] { })]
    public void TestBytesBidirectional(byte[] input)
    {
        TypeConverter td = TypeDescriptor.GetConverter(typeof(PyObject));

        Assert.True(td.CanConvertFrom(typeof(byte[])));

        using (GIL.Acquire())
        {
            using PyObject? pyObj = td.ConvertFrom(input) as PyObject;

            Assert.NotNull(pyObj);

            Assert.True(td.CanConvertTo(typeof(byte[])));

            // Convert back
            object? str = td.ConvertTo(pyObj, typeof(byte[]));
            Assert.Equal(input, str);
        }
    }

    /// Test that null converts into NoneType..
}
