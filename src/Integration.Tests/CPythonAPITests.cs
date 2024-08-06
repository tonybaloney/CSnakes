namespace Integration.Tests;

public class CPythonAPITests(TestEnvironment testEnv) : IClassFixture<TestEnvironment>
{
    TestEnvironment testEnv = testEnv;

    [Fact]
    public void PyVersion()
    {
        Assert.Contains("3.12", testEnv.Env.Version());
    }
}
