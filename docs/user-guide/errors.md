# Exception Handling

CSnakes will raise a `PythonInvocationException` if an error occurs during the execution of the Python code. The `PythonInvocationException` class contains the error message from the Python interpreter as the `InnerException`.

If the annotations are incorrect and your Python code returns a different type to what CSnakes was expecting, an `InvalidCastException` will be thrown with the details of the source and destination types.

## Fetching stack traces

You can fetch the Python stack trace as well as the Globals and Locals of the top frame by getting the `InnerException` attribute of the raised `PythonInvocationException`:

```csharp
try
{
  env.MethodToCall();
}
catch (PythonInvocationException ex)
{
  Console.WriteLine(ex.PythonExceptionType); // E.g. ValueError
  Console.WriteLine(ex.InnerException.PythonStackTrace); // IEnumerable<string> with the complete stack trace
  Console.WriteLine(ex.InnerException.Data["locals"]); // Dictionary <string, PyObject>
  Console.WriteLine(ex.InnerException.Data["globals"]); // Dictionary <string, PyObject>
}
```