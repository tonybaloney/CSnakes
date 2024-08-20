using CSnakes.Runtime.Python;

namespace Integration.Tests;
public class GeneratorTests: IntegrationTestBase
{
    [Fact]
    public void TestGenerator()
    {
        var mod = Env.TestGenerators();
        var iter = mod.ExampleGenerator(3);
        var generator = new GeneratorIterator<string, int, PyObject>(iter);

        Assert.True(generator.MoveNext());
        Assert.Equal("Item 0", generator.Current);
        Assert.Equal("Received 10", generator.Send(10));
        Assert.Equal(["Item 1", "Item 2"], generator.ToArray());

        generator.Close();
    }
}
