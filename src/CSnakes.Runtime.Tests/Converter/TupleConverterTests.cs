using CSnakes.Runtime.Python.Internals;

namespace CSnakes.Runtime.Tests.Converter;

public class Tuple1ConverterTests :
    ConverterTestBase<ValueTuple<long>,
                      PyObjectImporters.Tuple<long, PyObjectImporters.Int64>,
                      Tuple1ConverterTests>,
    IConverterTestCasesContainer<ValueTuple<long>>
{
    public static TheoryData<ValueTuple<long>> TestCases => [new(42)];
}

public class Tuple2ConverterTests :
    ConverterTestBase<(long, long),
        PyObjectImporters.Tuple<long, long, PyObjectImporters.Int64, PyObjectImporters.Int64>,
        Tuple2ConverterTests>,
    IConverterTestCasesContainer<(long, long)>
{
    public static TheoryData<(long, long)> TestCases => new() { (42, 42) };
}

public class Tuple3ConverterTests :
    ConverterTestBase<(long, long, long),
        PyObjectImporters.Tuple<long, long, long, PyObjectImporters.Int64, PyObjectImporters.Int64, PyObjectImporters.Int64>,
        Tuple3ConverterTests>,
    IConverterTestCasesContainer<(long, long, long)>
{
    public static TheoryData<(long, long, long)> TestCases => new() { (42, 42, 42) };
}

public class Tuple8ConverterTests :
    ConverterTestBase<(long, long, long, long, long, long, long, long),
        PyObjectImporters.Tuple<long, long, long, long, long, long, long, long,
            PyObjectImporters.Int64, PyObjectImporters.Int64, PyObjectImporters.Int64,
            PyObjectImporters.Int64, PyObjectImporters.Int64, PyObjectImporters.Int64,
            PyObjectImporters.Int64, PyObjectImporters.Int64>,
        Tuple8ConverterTests>,
    IConverterTestCasesContainer<(long, long, long, long, long, long, long, long)>
{
    public static TheoryData<(long, long, long, long, long, long, long, long)> TestCases =>
        new() { (1, 2, 3, 4, 5, 6, 7, 8) };
}

public class Tuple17ConverterTests :
    ConverterTestBase<(long, long, long, long, long, long, long, long, long, long, long, long, long, long, long, long, long),
        Tuple17ConverterTests>,
    IConverterTestCasesContainer<(long, long, long, long, long, long, long, long, long, long, long, long, long, long, long, long, long)>
{
    public static TheoryData<(long, long, long, long, long, long, long, long, long, long, long, long, long, long, long, long, long)>
        TestCases => new() { (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17) };
}
