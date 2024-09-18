namespace Integration.Tests;
public class GeneratorTests : IntegrationTestBase
{
    [Fact]
    public void TestGenerator()
    {
        var mod = Env.TestGenerators();
        var generator = mod.ExampleGenerator(3);

        Assert.True(generator.MoveNext());
        Assert.Equal("Item 0", generator.Current);
        Assert.Equal("Received 10", generator.Send(10));
        Assert.Equal<string[]>(["Item 1", "Item 2"], generator.ToArray());
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
