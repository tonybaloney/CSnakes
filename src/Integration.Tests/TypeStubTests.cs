namespace Integration.Tests;

public class TypeStubTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void TestHtml()
    {
        var mod = Env.Html();
        Assert.Equal("&lt;hello&gt;", mod.Escape("<hello>"));
    }
}
