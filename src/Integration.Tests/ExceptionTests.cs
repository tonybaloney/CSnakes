namespace Integration.Tests;

public class EndToEndTests : IntegrationTestBase
{

    [Fact]
    public void TestExceptions_TestRaisePythonException()
    {
        var testExceptionsModule = Env.TestExceptions();
        // This should fail. The python function raises an exception.
        var exception = Assert.Throws<PythonException>(testExceptionsModule.TestRaisePythonException);
        Assert.Equal("This is a Python exception", exception.Message);
        Assert.Equal("ValueError", exception.ExceptionType);
        Assert.Single(exception.PythonStackTrace);
        Assert.Contains("in test_raise_python_exception", exception.PythonStackTrace[0]);
    }
}
