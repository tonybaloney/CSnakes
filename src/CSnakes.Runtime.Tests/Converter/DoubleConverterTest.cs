using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Converter;

public class DoubleConverterTest :
    ConverterTestBase<double, PyObjectImporters.Double, DoubleConverterTest>,
    IConverterTestCasesContainer<double>
{
    public static TheoryData<double> TestCases => new()
    {
        0.0,
        2.0,
        -2.0,
        double.NegativeInfinity,
        double.PositiveInfinity
    };
}
