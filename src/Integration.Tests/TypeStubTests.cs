namespace Integration.Tests;

public class TypeStubTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void TestHtml()
    {
        var mod = CSnakes.Runtime.HtmlExtensions.Html(Env);
        Assert.Equal("&lt;hello&gt;", mod.Escape("<hello>"));

        Assert.Equal("<hello>", mod.Unescape("&lt;hello&gt;"));
    }

    [Fact]
    public void TestSubmoduleImport()
    {
        var mod = CSnakes.Runtime.Submodule.HtmlExtensions.Html(Env);
        Assert.NotNull(mod);

        Assert.True(mod.HtmlFunction().IsNone());
    }
}
