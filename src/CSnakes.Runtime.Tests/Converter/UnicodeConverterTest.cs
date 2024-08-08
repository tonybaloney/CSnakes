using CSnakes.Runtime.Python;
using System.ComponentModel;

namespace CSnakes.Runtime.Tests.Converter;

[Collection("ConversionTests")]
public class UnicodeConverterTest
{
    [Theory]
    [InlineData("Hello, World!")]
    [InlineData("你好，世界！")]
    [InlineData("こんにちは、世界！")]
    [InlineData("안녕하세요, 세계!")]
    [InlineData("مرحبا بالعالم!")]
    [InlineData("नमस्ते दुनिया!")]
    public void TestUnicodeBidirectional(string input)
    {
        var td = TypeDescriptor.GetConverter(typeof(PyObject));

        Assert.True(td.CanConvertTo(typeof(string)));

        using PyObject? pyObj = td.ConvertFromString(input) as PyObject;

        Assert.NotNull(pyObj);

        // Convert back
        string? str = td.ConvertToString(pyObj);
        Assert.Equal(input, str);
    }
}
