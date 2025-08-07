# API Reference

This section provides detailed API reference for CSnakes types, methods, and interfaces.

## Running any Python code

Sometimes you may want to run Python code that doesn't have type annotations or is not part of a module. You can use the `Run` method on the `IPythonEnvironment` to execute any Python code.

Python has two ways to do, this "expressions" and "statements". Expressions return a value and are a single line. You can use the `ExecuteExpression` method to run an expression and get the result back:

```csharp
env.ExecuteExpression("1+1").As<int>() ; // 2
```

You can pass a dictionary of local and/or global variables:

```csharp
var locals = new Dictionary<string, PyObject>
{
    ["a"] = PyObject.From(101)
};
using var result = env.ExecuteExpression("a+1", locals); // 102
```

To execute a series of statements, you can use the `Execute` method, which also takes globals and locals:

```csharp
var c = """
a = 101
b = c + a
""";
var locals = new Dictionary<string, PyObject>
{
    ["c"] = PyObject.From(101)
};
var globals = new Dictionary<string, PyObject>
{
    ["d"] = PyObject.From(100)
};
using var result = env.Execute(c, locals, globals);
Console.WriteLine(locals["b"].ToString()); // 202
```

The `locals` and `globals` dictionaries are mutable, so any changes to their values will be updated after the execution of the code.

## Next Steps

- [Learn about type mapping](type-mapping.md)
- [Explore configuration options](configuration.md)
- [Review error handling](../user-guide/errors.md)
