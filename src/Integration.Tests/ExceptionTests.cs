namespace Integration.Tests;

public class EndToEndTests : IntegrationTestBase
{

    [Fact]
    public void TestExceptions_TestRaisePythonException()
    {
        var testExceptionsModule = Env.TestExceptions();
        // This should fail. The python function raises an exception.
        Assert.Throws<PythonException>(testExceptionsModule.TestRaisePythonException);
    }
}
