using CSnakes.Runtime.Python;
using System.ComponentModel;

namespace CSnakes.Runtime.Tests.Converter;

public class LongConverterTest : ConverterTestBase
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(42)]
    [InlineData(-42)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    public void TestLongBidirectional(long input)
    {
        var td = TypeDescriptor.GetConverter(typeof(PyObject));

        Assert.True(td.CanConvertFrom(typeof(long)));

        using (GIL.Acquire())
        {
            using PyObject? pyObj = td.ConvertFrom(input) as PyObject;

            Assert.NotNull(pyObj);
            Assert.Equal(input.ToString(), pyObj.ToString());

            // Convert back
            object? str = td.ConvertTo(pyObj, typeof(long));
            Assert.Equal(input, str);
        }
    }
}
