# Type System

CSnakes provides seamless integration between Python and C# type systems through automatic type conversion and source generation.

## Supported Type Mappings

| Python Type | C# Type | Notes |
|-------------|---------|-------|
| `int` | `long` | Python integers can be arbitrarily large |
| `float` | `double` | IEEE 754 double precision |
| `str` | `string` | UTF-8 encoded strings |
| `bytes` | `byte[]` | Raw byte arrays |
| `bool` | `bool` | Boolean values |
| `list[T]` | `IReadOnlyList<T>` | Immutable list interface |
| `dict[K, V]` | `IReadOnlyDictionary<K, V>` | Immutable dictionary interface |
| `tuple[T1, T2, ...]` | `(T1, T2, ...)` | Value tuples |
| `None` (return) | `void` | No return value |

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

## Working with PyObject

For advanced scenarios, you can work directly with `PyObject`:

```python
from typing import Any

def get_raw_object() -> Any:
    return {"key": "value", "nested": [1, 2, 3]}
```

```csharp
using CSnakes.Runtime.Python;

PyObject obj = module.GetRawObject();

// Check type
if (obj.HasAttribute("keys"))
{
    // It's a dictionary-like object
    PyObject keys = obj.GetAttribute("keys");
    // ... work with the object
}
```

See [reference](../reference/api.md#pyobject) on PyObject for more details.

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
if (result.HasValue)
{
    Console.WriteLine($"Result: {result.Value}");
}
else
{
    Console.WriteLine("Division failed");
}
```

## Next Steps

- [Learn about environment management](environments.md)
- [Work with NumPy arrays and buffers](buffers.md)
- [Explore async functions](async.md)
