namespace CSnakes.Runtime.Tests.Converter;

public class FloatConverterTest :
    ConverterTestBase<float, FloatConverterTest>,
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
