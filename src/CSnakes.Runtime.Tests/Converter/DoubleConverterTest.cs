using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Converter;

public class DoubleConverterTest(PythonEnvironmentFixture fixture) :
    ConverterTestBase<double, PyObjectImporters.Double, DoubleConverterTest>(fixture),
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
