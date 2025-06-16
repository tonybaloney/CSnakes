namespace Integration.Tests;

public class IterableTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void TestBuiltinIterable()
    {
        var mod = Env.TestIterable();
        using var result = mod.TestBuiltinIterable();
        foreach (var item in result)
        {
            Assert.Equal("item", item);
        }
    }
}
