using CSnakes.Runtime;

namespace Integration.Tests;

public partial class EndToEndTests
{

    [Fact]
    public void TestExceptions_TestRaisePythonException()
    {
        var testExceptionsModule = testEnv.Env.TestExceptions();
        // This should fail. The python function raises an exception.
        Assert.Throws<PythonException>(testExceptionsModule.TestRaisePythonException);
    }
}
