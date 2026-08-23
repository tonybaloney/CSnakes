using CSnakes.Runtime.Python;

namespace CSnakes.Runtime.Tests.Converter;

public class Tuple1ConverterTests(PythonEnvironmentFixture fixture) :
    ConverterTestBase<ValueTuple<long>,
                      PyObjectImporters.Tuple<long, PyObjectImporters.Int64>,
                      Tuple1ConverterTests>(fixture),
    IConverterTestCasesContainer<ValueTuple<long>>
{
    public static TheoryData<ValueTuple<long>> TestCases => new() { ValueTuple.Create(1) };
}

public class Tuple2ConverterTests(PythonEnvironmentFixture fixture) :
    ConverterTestBase<(long, long),
        PyObjectImporters.Tuple<long, long, PyObjectImporters.Int64, PyObjectImporters.Int64>,
        Tuple2ConverterTests>(fixture),
    IConverterTestCasesContainer<(long, long)>
{
    public static TheoryData<(long, long)> TestCases => new() { (1, 2) };
}

public class Tuple3ConverterTests(PythonEnvironmentFixture fixture) :
    ConverterTestBase<(long, long, long),
        PyObjectImporters.Tuple<long, long, long, PyObjectImporters.Int64, PyObjectImporters.Int64, PyObjectImporters.Int64>,
        Tuple3ConverterTests>(fixture),
    IConverterTestCasesContainer<(long, long, long)>
{
    public static TheoryData<(long, long, long)> TestCases => new() { (1, 2, 3) };
}

public class Tuple8ConverterTests(PythonEnvironmentFixture fixture) :
    ConverterTestBase<(long, long, long, long, long, long, long, long),
        PyObjectImporters.Tuple<long, long, long, long, long, long, long, long,
            PyObjectImporters.Int64, PyObjectImporters.Int64, PyObjectImporters.Int64,
            PyObjectImporters.Int64, PyObjectImporters.Int64, PyObjectImporters.Int64,
            PyObjectImporters.Int64, PyObjectImporters.Int64>,
        Tuple8ConverterTests>(fixture),
    IConverterTestCasesContainer<(long, long, long, long, long, long, long, long)>
{
    public static TheoryData<(long, long, long, long, long, long, long, long)> TestCases =>
        new() { (1, 2, 3, 4, 5, 6, 7, 8) };
}

public class Tuple17ConverterTests(PythonEnvironmentFixture fixture) :
    ConverterTestBase<(long, long, long, long, long, long, long, long, long, long, long, long, long, long, long, long, long),
        Tuple17ConverterTests>(fixture),
    IConverterTestCasesContainer<(long, long, long, long, long, long, long, long, long, long, long, long, long, long, long, long, long)>
{
    public static TheoryData<(long, long, long, long, long, long, long, long, long, long, long, long, long, long, long, long, long)>
        TestCases => new() { (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17) };
}
