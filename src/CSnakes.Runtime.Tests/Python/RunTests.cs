namespace CSnakes.Runtime.Tests.Python;
public class RunTests : RuntimeTestBase
{
    [Fact]
    public void TestSimpleString()
    {
        using var result = env.Execute("1+1");
        Assert.Equal("2", result.ToString());
    }
}
