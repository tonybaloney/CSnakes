using CSnakes.Runtime.Python;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CSnakes.Runtime.Tests.Convertors;

[Collection("ConversionTests")]
public class TupleConverterTests
{
    [Fact]
    public void TupleConverter_SingleArgument()
    {
        Tuple<long> input = new(42);
        TupleTestInternal(input);
    }

    [Fact]
    public void TupleConverter_TwoArguments()
    {
        (long, long) input = (42, 42);
        TupleTestInternal(input);
    }

    [Fact]
    public void TupleConverter_ThreeArguments()
    {
        (long, long, long) input = (42, 42, 42);
        TupleTestInternal(input);
    }

    [Fact]
    public void TupleConverter_EightArguments()
    {
        (long, long, long, long, long, long, long, long) input = (1, 2, 3, 4, 5, 6, 7, 8);
        TupleTestInternal(input);
    }

    [Fact]
    public void TupleConverter_SeventeenArguments()
    {
        (long, long, long, long, long, long, long, long, long, long, long, long, long, long, long, long, long) input =
            (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17);
        TupleTestInternal(input);
    }

    private static void TupleTestInternal(ITuple input)
    {
        var td = TypeDescriptor.GetConverter(typeof(PyObject));
        Assert.True(td.CanConvertFrom(input.GetType()));

        var pyObj = td.ConvertFrom(input) as PyObject;
        Assert.NotNull(pyObj);

        // Assert.Equal(input.ToString(), pyObj.ToString());

        // Convert back
        var str = td.ConvertTo(pyObj, input.GetType());
        pyObj.Dispose();
        Assert.Equal(input, str);
    }
}
