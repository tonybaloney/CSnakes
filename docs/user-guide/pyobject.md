# Working with PyObject

The `PyObject` class is the foundation of Python interoperability in CSnakes. It represents a Python object and provides methods to interact with it directly from C#. This is useful for advanced scenarios where you need to work with Python objects that don't have direct C# type mappings.

## Creating PyObject Instances

### From .NET Values

You can create `PyObject` instances from various .NET types using the static `From` methods:

```csharp
// Basic types
PyObject pyString = PyObject.From("Hello, World!");
PyObject pyInt = PyObject.From(42);
PyObject pyFloat = PyObject.From(3.14);
PyObject pyBool = PyObject.From(true);
PyObject pyNone = PyObject.From(null);

// Collections
PyObject pyList = PyObject.From(new[] { 1, 2, 3, 4, 5 });
PyObject pyDict = PyObject.From(new Dictionary<string, object>
{
    ["name"] = "Alice",
    ["age"] = 30,
    ["active"] = true
});

// Tuples
PyObject pyTuple = PyObject.From((1, "hello", 3.14));
```

### None Values

The `PyObject.None` property provides access to Python's `None` value:

```csharp
PyObject noneValue = PyObject.None;
Console.WriteLine(noneValue.IsNone()); // True
```

## Object Comparisons

### Identity Comparison (`is` operator)

The equivalent to Python's `x is y` operator uses the `Is()` method:

```csharp
// Small numbers are the same object in Python (weird implementation detail)
PyObject obj1 = PyObject.From(42);
PyObject obj2 = PyObject.From(42);
if (obj1.Is(obj2))
    Console.WriteLine("Objects are the same instance!");

// None values are always the same instance
PyObject none1 = PyObject.None;
PyObject none2 = PyObject.None;
Console.WriteLine(none1.Is(none2)); // True
```

### Equality Comparison (`==` operator)

Equality can be checked using the `.Equals()` method or `==` operators:

```csharp
PyObject obj1 = PyObject.From(3.0);
PyObject obj2 = PyObject.From(3);
if (obj1 == obj2)
    Console.WriteLine("Objects are equal!"); // Python considers 3.0 == 3

// Collection equality
PyObject list1 = PyObject.From(new[] { "Hello!", "World!" });
PyObject list2 = PyObject.From(new[] { "Hello!", "World!" });
Console.WriteLine(list1.Equals(list2)); // True
```

### Inequality Comparison (`!=` operator)

Inequality can be checked using the `.NotEquals()` method or `!=` operators:

```csharp
PyObject obj1 = PyObject.From("Hello!");
PyObject obj2 = PyObject.From("Hello?");
if (obj1 != obj2)
    Console.WriteLine("Objects are not equal!");

// Using NotEquals method
if (obj1.NotEquals(obj2))
    Console.WriteLine("Objects are not equal!");
```

### Ordering Comparisons

PyObject supports ordering comparisons with `<`, `<=`, `>`, and `>=` operators:

```csharp
PyObject a = PyObject.From(5);
PyObject b = PyObject.From(10);

Console.WriteLine(a < b);  // True
Console.WriteLine(a <= b); // True
Console.WriteLine(a > b);  // False
Console.WriteLine(a >= b); // False

// String comparison
PyObject str1 = PyObject.From("apple");
PyObject str2 = PyObject.From("banana");
Console.WriteLine(str1 < str2); // True (lexicographic order)
```

## Boolean Context

### Truthiness Testing

You can test if a PyObject is truthy using the `true` operator or if statements:

```csharp
// Various truthy/falsy values
PyObject[] values = {
    PyObject.From(true),           // True
    PyObject.From(false),          // False
    PyObject.From(42),             // True
    PyObject.From(0),              // False
    PyObject.From("hello"),        // True
    PyObject.From(""),             // False
    PyObject.From(new[] { 1, 2 }), // True
    PyObject.From(Array.Empty<int>()), // False
    PyObject.None                  // False
};

foreach (var value in values)
{
    if (value)
        Console.WriteLine($"{value.GetRepr()} is truthy");
    else
        Console.WriteLine($"{value.GetRepr()} is falsy");
}
```

### Logical NOT

Use the `!` operator to check if an object is falsy:

```csharp
PyObject emptyList = PyObject.From(Array.Empty<int>());
if (!emptyList)
    Console.WriteLine("Empty list is falsy");

PyObject zero = PyObject.From(0);
if (!zero)
    Console.WriteLine("Zero is falsy");
```

## Object Inspection

### Getting Object Type

Get the Python type of an object:

```csharp
PyObject pyString = PyObject.From("Hello, World!");
PyObject stringType = pyString.GetPythonType();
Console.WriteLine(stringType.ToString()); // <class 'str'>
```

### String Representations

Get string representations using `ToString()` and `GetRepr()`:

```csharp
PyObject pyString = PyObject.From("hello");

// str() representation
Console.WriteLine(pyString.ToString()); // hello

// repr() representation
Console.WriteLine(pyString.GetRepr()); // 'hello'
```

### Attribute Access

Check for and access object attributes:

```csharp
PyObject pyString = PyObject.From("Hello, World!");

// Check if attribute exists
if (pyString.HasAttr("__doc__"))
{
    // Get attribute value
    PyObject docAttr = pyString.GetAttr("__doc__");
    Console.WriteLine(docAttr.ToString());
}

// Access string methods
PyObject upperMethod = pyString.GetAttr("upper");
PyObject upperResult = upperMethod.Call();
Console.WriteLine(upperResult.ToString()); // HELLO, WORLD!
```

## Method Calls

### Simple Method Calls

Call Python methods without arguments:

```csharp
PyObject pyList = PyObject.From(new[] { 1, 2, 3 });
PyObject copyMethod = pyList.GetAttr("copy");
PyObject copiedList = copyMethod.Call();
```

### Method Calls with Arguments

Call Python methods with positional arguments:

```csharp
PyObject pyList = PyObject.From(new[] { 1, 2, 3 });
PyObject appendMethod = pyList.GetAttr("append");

// Append a value
appendMethod.Call(PyObject.From(4));
Console.WriteLine(pyList.ToString()); // [1, 2, 3, 4]
```

### Method Calls with Keyword Arguments

Call methods with both positional and keyword arguments:

```csharp
// Example: str.replace(old, new, count=1)
PyObject pyString = PyObject.From("hello world hello");
PyObject replaceMethod = pyString.GetAttr("replace");

PyObject result = replaceMethod.CallWithKeywordArguments(
    args: new[] { PyObject.From("hello"), PyObject.From("hi") },
    kwnames: new[] { "count" },
    kwvalues: new[] { PyObject.From(1) }
);

Console.WriteLine(result.ToString()); // hi world hello
```

## Type Conversion

### Converting to .NET Types

Convert PyObject instances back to .NET types using the `As<T>()` method:

```csharp
// Create Python objects
PyObject pyInt = PyObject.From(42);
PyObject pyString = PyObject.From("Hello");
PyObject pyList = PyObject.From(new[] { 1, 2, 3 });

// Convert back to .NET types
long intValue = pyInt.As<long>();
string stringValue = pyString.As<string>();
IReadOnlyList<object> listValue = pyList.As<IReadOnlyList<object>>();

Console.WriteLine($"Integer: {intValue}");
Console.WriteLine($"String: {stringValue}");
Console.WriteLine($"List: [{string.Join(", ", listValue)}]");
```

### Safe Type Conversion

Handle conversion errors with try-catch blocks:

```csharp
PyObject unknownObject = GetSomePythonObject();

try
{
    var stringValue = unknownObject.As<string>();
    Console.WriteLine($"Got string: {stringValue}");
}
catch (InvalidCastException)
{
    try
    {
        var longValue = unknownObject.As<long>();
        Console.WriteLine($"Got number: {longValue}");
    }
    catch (InvalidCastException)
    {
        Console.WriteLine("Could not convert to string or number");
    }
}
```

## Iteration

### Iterating Over Python Objects

Use `AsEnumerable<T>()` to iterate over Python sequences:

```csharp
PyObject pyList = PyObject.From(new[] { "apple", "banana", "cherry" });

// Iterate as strings
foreach (string item in pyList.AsEnumerable<string>())
{
    Console.WriteLine(item);
}

// Iterate as PyObjects for more control
foreach (PyObject item in pyList.AsEnumerable<PyObject>())
{
    Console.WriteLine($"Item: {item.GetRepr()}, Type: {item.GetPythonType()}");
}
```

### Working with Generators

Iterate over Python generators and iterators:

```csharp
// Assuming you have a Python function that returns a generator
var pythonModule = env.SomeModule();
PyObject generator = pythonModule.GetGenerator();

foreach (var item in generator.AsEnumerable<int>())
{
    Console.WriteLine($"Generated: {item}");
}
```

## Memory Management

### Resource Disposal

PyObject implements `IDisposable` and should be properly disposed:

```csharp
// Using 'using' statements (recommended)
using PyObject pyObj = PyObject.From("Hello, World!");
Console.WriteLine(pyObj.ToString());
// Object is automatically disposed here

// Manual disposal
PyObject pyObj2 = PyObject.From(42);
try
{
    Console.WriteLine(pyObj2.ToString());
}
finally
{
    pyObj2.Dispose();
}
```

### Cloning Objects

Create copies of PyObject instances:

```csharp
PyObject original = PyObject.From("Hello");
PyObject cloned = original.Clone();

// Both objects refer to the same Python string object
Console.WriteLine(original.Is(cloned)); // True for immutable objects
```

## Hash Codes

Get hash codes for use in .NET collections:

```csharp
PyObject obj1 = PyObject.From("hello");
PyObject obj2 = PyObject.From("world");

var hashSet = new HashSet<PyObject> { obj1, obj2 };
Console.WriteLine($"HashSet contains {hashSet.Count} items");

// Hash codes match Python's hash() function
Console.WriteLine($"Hash of 'hello': {obj1.GetHashCode()}");
```

## Error Handling

### Python Exception Handling

When PyObject operations fail, they throw `PythonInvocationException`:

```csharp
try
{
    PyObject pyObj = PyObject.From(42);
    // This will fail because integers don't have an 'invalid_method'
    PyObject method = pyObj.GetAttr("invalid_method");
}
catch (PythonInvocationException ex)
{
    Console.WriteLine($"Python error: {ex.PythonExceptionType}");
    Console.WriteLine($"Message: {ex.Message}");
}
```

### Null Checking

Always check for null when working with PyObject:

```csharp
PyObject? result = GetOptionalPythonObject();
if (result != null)
{
    using (result)
    {
        Console.WriteLine(result.ToString());
    }
}
```

## Best Practices

### 1. Always Use `using` Statements

```csharp
// Good - automatic disposal
using PyObject pyObj = PyObject.From("Hello");
Console.WriteLine(pyObj.ToString());

// Avoid - manual disposal required
PyObject pyObj2 = PyObject.From("Hello");
// ... easy to forget to dispose
```

### 2. Check Object Types Before Conversion

```csharp
PyObject unknownObj = GetPythonObject();

// Check the type first
PyObject objType = unknownObj.GetPythonType();
string typeName = objType.GetAttr("__name__").ToString();

if (typeName == "str")
{
    string value = unknownObj.As<string>();
    // Work with string
}
else if (typeName == "int")
{
    long value = unknownObj.As<long>();
    // Work with integer
}
```

### 3. Handle None Values

```csharp
PyObject result = CallPythonFunction();

if (result.IsNone())
{
    Console.WriteLine("Function returned None");
}
else
{
    // Process the actual result
    ProcessResult(result);
}
```

### 4. Use Appropriate Collection Types

```csharp
// For lists - use IReadOnlyList<T>
PyObject pyList = PyObject.From(new[] { 1, 2, 3 });
IReadOnlyList<long> list = pyList.As<IReadOnlyList<long>>();

// For dictionaries - use IReadOnlyDictionary<K, V>
PyObject pyDict = PyObject.From(new Dictionary<string, int> { ["a"] = 1 });
IReadOnlyDictionary<string, long> dict = pyDict.As<IReadOnlyDictionary<string, long>>();
```

## Next Steps

- [Learn about type conversions](type-system.md)
- [Work with async functions](async.md)
- [Handle errors gracefully](errors.md)
