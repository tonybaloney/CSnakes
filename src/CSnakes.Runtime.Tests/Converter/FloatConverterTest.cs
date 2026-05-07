namespace CSnakes.Runtime.Tests.Converter;

public class FloatConverterTest(PythonEnvironmentFixture fixture) :
    ConverterTestBase<float, FloatConverterTest>(fixture),
    IConverterTestCasesContainer<float>
{
    public static TheoryData<float> TestCases => new()
    {
        0.0f,
        2.0f,
        -2.0f,
        float.NegativeInfinity,
        float.PositiveInfinity
    };
}
