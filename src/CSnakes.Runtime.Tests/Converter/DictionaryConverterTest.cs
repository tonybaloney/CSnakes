using CSnakes.Runtime.Python;
using System.ComponentModel;

namespace CSnakes.Runtime.Tests.Converter;

[Collection("ConversionTests")]
public class DictionaryConverterTest
{
    [Fact]
    public void DictionaryConverter()
    {
        Dictionary<string, string> input = new()
        {
            ["Hello"] = "World",
            ["Foo"] = "Bar"
        };
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
