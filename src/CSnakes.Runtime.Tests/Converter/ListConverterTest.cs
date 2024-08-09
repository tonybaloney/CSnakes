using CSnakes.Runtime.Python;
using System.ComponentModel;

namespace CSnakes.Runtime.Tests.Converter;

public class ListConverterTest : ConverterTestBase
{
    [Fact]
    public void IEnumerableConverter()
    {
        IEnumerable<string> input = ["Hell0", "W0rld"];

        var td = TypeDescriptor.GetConverter(typeof(PyObject));

        Assert.True(td.CanConvertFrom(input.GetType()));
        using (GIL.Acquire())
        {
            using PyObject? pyObj = td.ConvertFrom(input) as PyObject;

            Assert.NotNull(pyObj);

            Assert.True(td.CanConvertTo(input.GetType()));

            // Convert back
            object? str = td.ConvertTo(pyObj, input.GetType());
            Assert.Equal(input, str);
        }
    }

    [Fact]
    public void ListConverter()
    {
        List<long> input = [123456, 123562];

        var td = TypeDescriptor.GetConverter(typeof(PyObject));

        Assert.True(td.CanConvertFrom(input.GetType()));
        using (GIL.Acquire())
        {
            using PyObject? pyObj = td.ConvertFrom(input) as PyObject;

            Assert.NotNull(pyObj);

            Assert.True(td.CanConvertTo(input.GetType()));

            // Convert back
            object? str = td.ConvertTo(pyObj, input.GetType());
            Assert.Equal(input, str);
        }
    }
}
