# Type System

CSnakes provides seamless integration between Python and C# type systems through automatic and manual type conversion.

In Python, every object inherits from the base object type (`PyObject`). CSnakes has a `PyObject` class that represents all Python objects in C#. This class provides methods for attribute access, method invocation, and function calls, allowing you to interact with Python objects as if they were native C# objects.

The `PyObject` class in CSnakes has conversions to and from many C# types. You can also do anything you can do in Python (get attributes, call methods, call functions) on instances of `PyObject`

To make development easier, the CSnakes source generator generates the conversions (marshalling) calls to and from Python functions by using their type signatures.

Any of the supported types can be used in the `PyObject.From<T>(T object)` and `T PyObject.As<T>(PyObject x)` calls to marshal data from C# types into Python objects. `PyObject` instances contain a `SafeHandle` to the allocated memory for the Python object in the Python interpreter. When the object is disposed in C#, the reference is decremented and the object is released. C# developers don't need to worry about manually incrementing and decrementing references to Python objects, they can work within the design of the existing .NET Garbage Collector.

## Supported Type Mappings

CSnakes supports the following typed scenarios:

| Python type annotation | Reflected C# Type |
|------------------------|-------------------|
| `int`                  | `long`            |
| `float`                | `double`          |
| `str`                  | `string`          |
| `bytes`                | `byte[]`          |
| `bool`                 | `bool`            |
| `list[T]`              | `IReadOnlyList<T>`  |
| `dict[K, V]`           | `IReadOnlyDictionary<K, V>` |
| `tuple[T1, T2, ...]`   | `(T1, T2, ...)`   |
| `typing.Sequence[T]`   | `IReadOnlyList<T>`  |
| `typing.Dict[K, V]`    | `IReadOnlyDictionary<K, V>` |
| `typing.Mapping[K, V]` | `IReadOnlyDictionary<K, V>` |
| `typing.Tuple[T1, T2, ...]` | `(T1, T2, ...)` |
| `typing.Optional[T]`   | `T?`              |
| `T | None`             | `T?`              |
| `typing.Generator[TYield, TSend, TReturn]` | `IGeneratorIterator<TYield, TSend, TReturn>` |
| `typing.Buffer`        | `IPyBuffer` [2](buffers.md) |
| `typing.Coroutine[TYield, TSend, TReturn]` | `Task<TYield>` [3](async.md) |
| `None` (Return)        | `void`            |

## Optional Types

CSnakes supports Python's optional type annotations from both [`Optional[T]`](https://docs.python.org/3/library/typing.html#typing.Optional) and `T | None` (Python 3.10+):

```python
def find_user(user_id: int) -> str | None:
    # Returns username or None if not found
    users = {1: "Alice", 2: "Bob"}
    return users.get(user_id)

def process_optional(value: int | None = None) -> str:
    if value is None:
        return "No value provided"
    return f"Value is {value}"

def optional_old_style(value: Optional[int] = None) -> None:
    pass
```

Generated C# signatures:

```csharp
public string? FindUser(long userId);
public string ProcessOptional(long? value = null);
public void OptionalOldStyle(long? value = null);
```

## Collections

### Lists

```python
def process_numbers(numbers: list[int]) -> list[str]:
    return [str(n * 2) for n in numbers]

def filter_positive(numbers: list[float]) -> list[float]:
    return [n for n in numbers if n > 0]
```

```csharp
var numbers = new[] { 1, 2, 3, 4, 5 };
IReadOnlyList<string> result = module.ProcessNumbers(numbers);
// Result: ["2", "4", "6", "8", "10"]

var floats = new[] { -1.5, 2.3, -0.1, 4.7 };
IReadOnlyList<double> positive = module.FilterPositive(floats);
// Result: [2.3, 4.7]
```

### Dictionaries

```python
def word_count(text: str) -> dict[str, int]:
    words = text.split()
    return {word: words.count(word) for word in set(words)}

def user_lookup() -> dict[int, str]:
    return {1: "Alice", 2: "Bob", 3: "Charlie"}
```

```csharp
var text = "hello world hello";
IReadOnlyDictionary<string, long> counts = module.WordCount(text);
// Result: {"hello": 2, "world": 1}

var users = module.UserLookup();
string userName = users[1]; // "Alice"
```

### Tuples

CSnakes supports simple tuples as types up to 17 items:

```python
def get_name_age() -> tuple[str, int]:
    return ("Alice", 30)

def get_coordinates() -> tuple[float, float, float]:
    return (12.34, 56.78, 90.12)

```

```csharp
var (name, age) = module.GetNameAge();
Console.WriteLine($"{name} is {age} years old");

var (x, y, z) = module.GetCoordinates();
Console.WriteLine($"Position: ({x}, {y}, {z})");
```

## Default Values

Python default values for types which support compile-time constants in C# (string, int, float, bool) are preserved in the generated C# methods:

```python
def greet(name: str, greeting: str = "Hello", punctuation: str = "!") -> str:
    return f"{greeting}, {name}{punctuation}"

def calculate(value: float, multiplier: float = 2.0, add_value: int = 0) -> float:
    return value * multiplier + add_value
```

Generated C# methods:

```csharp
public string Greet(string name, string greeting = "Hello", string punctuation = "!");
public double Calculate(double value, double multiplier = 2.0, long addValue = 0);
```

Usage:

```csharp
// Use all defaults
string msg1 = module.Greet("Alice"); // "Hello, Alice!"

// Override some defaults
string msg2 = module.Greet("Bob", "Hi"); // "Hi, Bob!"

// Override all parameters
string msg3 = module.Greet("Charlie", "Hey", "?"); // "Hey, Charlie?"
```

## Unsupported Types

See [Roadmap](../community/roadmap.md) for a list of unsupported types and possible alternatives.

## Handling None

If you need to send `None` as a `PyObject` to any function call from C#, use the property `PyObject.None`:

```csharp
env.MethodToCall(PyObject.None);
```

You can also check if a PyObject is None by calling `IsNone()` on any PyObject:

```csharp
PyObject obj = env.MethodToCall();
if (obj.IsNone())
{
  Console.WriteLine("The object is None");
}
```

Python's type system is unconstrained, so even though a function can say it returns a `int` it can return `None` object. Sometimes it's also useful to check for `None` values.

## Working with PyObject

For advanced scenarios, you can work directly with `PyObject`:

```python
from typing import Any

def get_person() -> Any:
    return ... # an object of some sort
```

```csharp
using CSnakes.Runtime.Python;

using PyObject obj = module.GetPerson();

// Check type
if (obj.HasAttr("keys"))
{
    // It's a dictionary-like object
    PyObject keys = obj.GetAttr("keys");
    // ... work with the object
}
```

See [handling Python Objects](pyobject.md) for more details and examples.

## Typing Third-Party Packages with Type Stubs

Sometimes you want to use a package which doesn't have type information. Python provides the ability to describe the types and function
signatures for a package using ["type-stubs" in the form of `.pyi` files](https://typing.python.org/en/latest/spec/distributing.html#stub-files).

CSnakes will generate bindings for `.pyi` files in your project whether included [manually or automatically](configuration.md#discovering-python-files-for-source-generation).

Python type stubs have no implementation for methods, functions, or attributes. Take this example:

```python
def create(name: str, count: int) -> list[str]:
    ...
```

The ellipsis (`...`) denotes this function has no implementation.

This works best with [Root Namespaces](configuration.md#namespaces-and-roots-optional).

IF there were a library called `http_client` which you want to use specific functions from, you could add a file, `http_client.pyi` to your .NET project:

```python
def get(url: str) -> dict[str, Any]:
    ...

def post(url: str, data: dict[str, Any]) -> dict[str, Any]:
    ...

def delete(url: str) -> dict[str, Any]:
    ...
```

Then CSnakes would generate a module for `http_client` called `HttpClient` with the methods:

```csharp
    public IReadOnlyDictionary<string, object> Get(string url);
    public IReadOnlyDictionary<string, object> Post(string url, IReadOnlyDictionary<string, object> data);
    public IReadOnlyDictionary<string, object> Delete(string url);
```

When calling those methods, it would `import http_client`, so as long as that package is installed into the Python environment the functions will be available.

## Best Practices

### 1. Use Specific Types

```python
# Good - specific types
def process_user_data(user_id: int, email: str) -> dict[str, str]:
    return {"id": str(user_id), "email": email}

# Avoid - too generic
def process_data(data: object) -> object:
    return data
```

### 2. Document Complex Return Types

```python
def get_analysis_results() -> dict[str, list[tuple[str, float]]]:
    """
    Returns analysis results.
    
    Returns:
        Dictionary mapping category names to lists of (item_name, score) tuples.
    """
    return {
        "positive": [("item1", 0.8), ("item2", 0.9)],
        "negative": [("item3", 0.2)]
    }
```

### 3. Handle None Values

```python
def safe_divide(a: float, b: float) -> float | None:
    return a / b if b != 0 else None
```

```csharp
double? result = module.SafeDivide(10.0, 3.0);
if (result is null)
{
    Console.WriteLine("Division failed");
}
else
{
    Console.WriteLine($"Result: {result}");
}
```

## Next Steps

- [Learn about environment management](environments.md)
- [Work with NumPy arrays and buffers](buffers.md)
- [Explore async functions](async.md)
