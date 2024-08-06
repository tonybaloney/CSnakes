namespace Integration.Tests;

public class CPythonAPITests(TestEnvironment testEnv) : IClassFixture<TestEnvironment>
{
    TestEnvironment testEnv = testEnv;

    [Fact]
    public void PyVersion()
    {
    }
}
