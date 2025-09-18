using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Converter;
public interface IConverterTestCasesContainer<T>
{
    static abstract TheoryData<T> TestCases { get; }
}

public abstract class ConverterTestBase<T, TTestCases> : RuntimeTestBase
    where TTestCases : IConverterTestCasesContainer<T>
{
    public static TheoryData<T> SubTestCases => TTestCases.TestCases;

    protected static void TestRoundtrip(T input, Func<PyObject, T> converter)
    {
        using PyObject pyObj = PyObject.From(input);
        Assert.NotNull(pyObj);

        // Convert back
        T actual = converter(pyObj);
        Assert.Equal(input, actual);
    }

    [Theory]
    [MemberData(nameof(SubTestCases), DisableDiscoveryEnumeration = true)]
    public void RoundtripViaAs(T input) =>
        TestRoundtrip(input, static obj => obj.As<T>());
}

public abstract class ConverterTestBase<T, TImporter, TTestCases> :
    ConverterTestBase<T, TTestCases>
    where TImporter : IPyObjectImporter<T>
    where TTestCases : IConverterTestCasesContainer<T>
{
    [Theory]
    [MemberData(nameof(SubTestCases),DisableDiscoveryEnumeration = true)]
    public void RoundtripViaImport(T input) =>
        TestRoundtrip(input, static obj => obj.ImportAs<T, TImporter>());
}
