namespace Integration.Tests;
public class GeneratorTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void TestGenerator()
    {
        var mod = Env.TestGenerators();
        var generator = mod.ExampleGenerator(3);

        Assert.True(generator.MoveNext());
        Assert.Equal("Item 0", generator.Current);
        Assert.True(generator.Send(10));
        Assert.Equal("Received 10", generator.Current);
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

    [Fact]
    public void TestGeneratorAsCallback()
    {
        var mod = Env.TestGenerators();
        var generator = mod.ExampleGenerator(3);

        var callback = new Action<string>(
            // This is the callback that will be called from the generator
            s => Assert.Equal(generator.Current, s)
        );

        // Wait for the generator to finish
        var task = Task.Run(() =>
        {
            while (generator.MoveNext())
            {
                // Simulate a callback
                callback(generator.Current);
                // Optionally send a value back to the generator
                generator.Send(10);
            }
            // Optionally return a value from the generator
            Assert.True(generator.Return);
        });
    }
}
