using CSnakes.Runtime.Python;
using System.ComponentModel;

namespace CSnakes.Runtime.Tests.Converter;

[Collection("ConversionTests")]
public class ListConverterTest
{
    [Fact]
    public void IEnumerableConverter()
    {
        IEnumerable<string> input = ["Hello", "World"];

        var td = TypeDescriptor.GetConverter(typeof(PyObject));

        Assert.True(td.CanConvertFrom(input.GetType()));

        var pyObj = td.ConvertFrom(input) as PyObject;

        Assert.NotNull(pyObj);

        Assert.True(td.CanConvertTo(input.GetType()));

        // Convert back
        var str = td.ConvertTo(pyObj, input.GetType());
        Assert.Equal(input, str);
        pyObj.Dispose();
    }

    [Fact]
    public void ListConverter()
    {
        List<string> input = ["Hello", "World"];

        var td = TypeDescriptor.GetConverter(typeof(PyObject));

        Assert.True(td.CanConvertFrom(input.GetType()));

        var pyObj = td.ConvertFrom(input) as PyObject;

        Assert.NotNull(pyObj);

        Assert.True(td.CanConvertTo(input.GetType()));

        // Convert back
        var str = td.ConvertTo(pyObj, input.GetType());
        Assert.Equal(input, str);

        pyObj.Dispose();
    }
}
