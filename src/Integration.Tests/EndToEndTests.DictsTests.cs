using CSnakes.Runtime;

namespace Integration.Tests;
public partial class EndToEndTests
{
    [Fact]
    public void TestDicts_TestDictStrInt()
    {
        var testDicts = testEnv.Env.TestDicts();

        IReadOnlyDictionary<string, long> testDict = new Dictionary<string, long> { { "hello", 1 }, { "world", 2 } };
        var result = testDicts.TestDictStrInt(testDict);
        Assert.Equal(1, result["hello"]);
        Assert.Equal(2, result["world"]);
    }

    [Fact]
    public void TestDicts_TestDictStrListInt()
    {
        var testDicts = testEnv.Env.TestDicts();

        IReadOnlyDictionary<string, IEnumerable<long>> testListDict = new Dictionary<string, IEnumerable<long>> { { "hello", new List<long> { 1, 2, 3 } }, { "world", new List<long> { 4, 5, 6 } } };
        var result = testDicts.TestDictStrListInt(testListDict);
        Assert.Equal(new List<long> { 1, 2, 3 }, result["hello"]);
        Assert.Equal(new List<long> { 4, 5, 6 }, result["world"]);
    }

    [Fact]
    public void TestDicts_TestDictStrDictInt()
    {
        var testDicts = testEnv.Env.TestDicts();

        IReadOnlyDictionary<string, IReadOnlyDictionary<string, long>> testDictDict = new Dictionary<string, IReadOnlyDictionary<string, long>> { { "hello", new Dictionary<string, long> { { "world", 1 } } } };
        var result = testDicts.TestDictStrDictInt(testDictDict);
        Assert.Equal(1, result["hello"]["world"]);
    }
}
