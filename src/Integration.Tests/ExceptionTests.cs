using CSnakes.Runtime.Python;

namespace Integration.Tests;

public class ExceptionTests : IntegrationTestBase
{

    [Fact]
    public void TestExceptions_TestRaisePythonException()
    {
        var testExceptionsModule = Env.TestExceptions();
        // This should fail. The python function raises an exception.
        var exception = Assert.Throws<PythonInvocationException>(testExceptionsModule.TestRaisePythonException);
        Assert.NotNull(exception.InnerException);
        Assert.Equal("This is a Python exception", exception.InnerException.Message);
        Assert.Equal("ValueError", exception.PythonExceptionType);
        var pythonRuntimeException = (PythonRuntimeException)exception.InnerException;
        Assert.Single(pythonRuntimeException.PythonStackTrace);
        Assert.Contains("in test_raise_python_exception", pythonRuntimeException.PythonStackTrace[0]);
        // Get the frame locals from the inner exception
        Assert.NotNull(pythonRuntimeException.Data["locals"]);
        var topFrameLocals = (IReadOnlyDictionary<string, PyObject>) pythonRuntimeException.Data["locals"];
        Assert.NotNull(topFrameLocals);
        Assert.Equal(1, topFrameLocals["a"].As<long>());
        Assert.Equal(2, topFrameLocals["b"].As<long>());
        var topFrameGlobals = (IReadOnlyDictionary<string, PyObject>)pythonRuntimeException.Data["globals"];
        Assert.Equal(topFrameGlobals["some_global"].ToString(), "1337");
    }
}
