namespace Integration.Tests;
public class TestDicts : IntegrationTestBase
{
    [Fact]
    public void TestDicts_TestDictStrInt()
    {
        var testDicts = Env.TestDicts();

        IReadOnlyDictionary<string, long> testDict = new Dictionary<string, long> { { "dictkey1", 1 }, { "dictkey2", 2 } };
        var result = testDicts.TestDictStrInt(testDict);
        Assert.Equal(1, result["dictkey1"]);
        Assert.Equal(2, result["dictkey2"]);
    }

    [Fact]
    public void TestDicts_TestDictStrListInt()
    {
        var testDicts = Env.TestDicts();

        IReadOnlyDictionary<string, IReadOnlyCollection<long>> testListDict = new Dictionary<string, IReadOnlyCollection<long>> { { "hello", new List<long> { 1, 2, 3 } }, { "world2", new List<long> { 4, 5, 6 } } };
        var result = testDicts.TestDictStrListInt(testListDict);
        Assert.Equal(new List<long> { 1, 2, 3 }, result["hello"]);
        Assert.Equal(new List<long> { 4, 5, 6 }, result["world2"]);
    }

    [Fact]
    public void TestDicts_TestDictStrDictInt()
    {
        var testDicts = Env.TestDicts();

        IReadOnlyDictionary<string, IReadOnlyDictionary<string, long>> testDictDict = new Dictionary<string, IReadOnlyDictionary<string, long>> { { "hello", new Dictionary<string, long> { { "world3", 1 } } } };
        var result = testDicts.TestDictStrDictInt(testDictDict);
        Assert.Equal(1, result["hello"]["world3"]);
    }

    [Fact]
    public void TestDicts_TestMapping()
    {
        var testDicts = Env.TestDicts();

        IReadOnlyDictionary<string, IReadOnlyDictionary<string, long>> testDictDict = new Dictionary<string, IReadOnlyDictionary<string, long>> { { "hello", new Dictionary<string, long> { { "world3", 1 } } } };
        var result = testDicts.TestMapping(testDictDict);
        Assert.Equal(1, result["hello"]["world3"]);
    }
}
