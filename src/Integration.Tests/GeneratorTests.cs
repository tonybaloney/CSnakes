namespace Integration.Tests;
public class GeneratorTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void TestGenerator()
    {
        var mod = Env.TestGenerators();
        var generator = mod.ExampleGenerator(3);

        Assert.True(generator.MoveNext());
        Assert.Equal("Item 0", ((IEnumerator<string>)generator).Current);
        Assert.True(generator.Send(10));
        Assert.Equal("Received 10", ((IEnumerator<string>)generator).Current);
        Assert.Equal<string[]>(["Item 1", "Item 2"], generator.ToArray());
        Assert.True(generator.Return);
    }

    [Fact]
    public void TestNormalGenerator()
    {
        // Test the most likely scenario of TSend and TReturn being None
        var mod = Env.TestGenerators();
        var generator = mod.TestNormalGenerator();
        Assert.Equal<string[]>(["one", "two"], generator.ToArray());
    }
}
