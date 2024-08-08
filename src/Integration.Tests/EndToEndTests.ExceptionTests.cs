using CSnakes.Runtime;

namespace Integration.Tests;

public partial class EndToEndTests
{

    [Fact]
    public void TestExceptions_TestRaisePythonException()
    {
        var testModule = testEnv.Env.TestExceptions();
        // This should fail. The python function raises an exception.
        Assert.Equal(1, testModule.TestRaisePythonException());
    }
}
