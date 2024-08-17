namespace Integration.Tests;
public class ArgsTests : IntegrationTestBase
{
    [Fact]
    public void PositionalOnly()
    {
        var mod = Env.TestArgs();
        Assert.Equal(6, mod.PositionalOnlyArgs(1, 2, 3));
    }

    [Fact]
    public void KeywordOnly()
    {
        var mod = Env.TestArgs();
        Assert.Equal(6, mod.KeywordOnlyArgs(1, b: 2, c: 3));
    }
}
