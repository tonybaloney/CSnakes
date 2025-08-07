# Type Mapping Reference

This reference provides comprehensive details about how Python types are mapped to C# types in CSnakes.

## Basic Type Mappings

### Primitive Types

| Python Type | C# Type | Size | Range/Notes |
|-------------|---------|------|-------------|
| `bool` | `bool` | 1 byte | `True`/`False` ↔ `true`/`false` |
| `int` | `long` | 8 bytes | Python int can be arbitrarily large |
| `float` | `double` | 8 bytes | IEEE 754 double precision |
| `str` | `string` | Variable | UTF-8 encoded Unicode strings |
| `bytes` | `byte[]` | Variable | Raw byte arrays |
| `None` | N/A (void) | - | Return type only |

### Large Integer Handling

Python's `int` type can represent arbitrarily large integers, while C#'s `long` is limited to 64 bits:

```python
# Python - handles large numbers
def get_large_number() -> int:
    return 123456789012345678901234567890
```

```csharp
// C# - May throw overflow exception for very large numbers
try
{
    var largeNum = module.GetLargeNumber(); // May fail
}
catch (OverflowException)
{
    // Handle large integer overflow
}
```

**Solution**: Use `PyObject` directly for very large integers:

```python
def get_large_number_safe() -> object:
    return 123456789012345678901234567890
```

```csharp
using var pyObj = module.GetLargeNumberSafe();
var bigInteger = pyObj.As<System.Numerics.BigInteger>();
```

## Collection Types

### Lists

| Python Type | C# Type | Mutability | Notes |
|-------------|---------|------------|-------|
| `list[T]` | `IReadOnlyList<T>` | Immutable view | List contents converted to C# types |
| `typing.Sequence[T]` | `IReadOnlyList<T>` | Immutable view | Generic sequence interface |

```python
def get_numbers() -> list[int]:
    return [1, 2, 3, 4, 5]

def get_names() -> list[str]:
    return ["Alice", "Bob", "Charlie"]

def get_mixed_list() -> list[str | int]:
    return ["Alice", 25, "Bob", 30]  # Mixed types
```

```csharp
// Homogeneous lists
IReadOnlyList<long> numbers = module.GetNumbers();
IReadOnlyList<string> names = module.GetNames();

// Mixed type lists become list of objects
IReadOnlyList<object> mixed = module.GetMixedList();
```

### Dictionaries

| Python Type | C# Type | Mutability | Notes |
|-------------|---------|------------|-------|
| `dict[K, V]` | `IReadOnlyDictionary<K, V>` | Immutable view | Key-value pairs converted |
| `typing.Dict[K, V]` | `IReadOnlyDictionary<K, V>` | Immutable view | Legacy typing syntax |
| `typing.Mapping[K, V]` | `IReadOnlyDictionary<K, V>` | Immutable view | Generic mapping interface |

```python
def get_user_info() -> dict[str, str]:
    return {"name": "Alice", "email": "alice@example.com"}

def get_scores() -> dict[str, int]:
    return {"math": 95, "science": 87, "english": 92}

def get_mixed_dict() -> dict[str, str | int]:
    return {"name": "Alice", "age": 30}
```

```csharp
// Strongly typed dictionaries
IReadOnlyDictionary<string, string> userInfo = module.GetUserInfo();
IReadOnlyDictionary<string, long> scores = module.GetScores();

// Mixed value types become object
IReadOnlyDictionary<string, object> mixed = module.GetMixedDict();

// Accessing values
string name = userInfo["name"];
long mathScore = scores["math"];
```

### Tuples

| Python Type | C# Type | Notes |
|-------------|---------|-------|
| `tuple[T1, T2]` | `(T1, T2)` | Value tuple with 2 elements |
| `tuple[T1, T2, T3]` | `(T1, T2, T3)` | Value tuple with 3 elements |
| `tuple[T1, ..., Tn]` | `(T1, ..., Tn)` | Up to 8 elements supported |
| `typing.Tuple[T1, T2]` | `(T1, T2)` | Legacy typing syntax |

```python
def get_coordinates() -> tuple[float, float]:
    return (12.34, 56.78)

def get_user_record() -> tuple[str, int, bool]:
    return ("Alice", 30, True)

def get_name_parts() -> tuple[str, str]:
    full_name = "John Doe"
    parts = full_name.split(" ", 1)
    return (parts[0], parts[1] if len(parts) > 1 else "")
```

```csharp
// Destructuring tuples
var (x, y) = module.GetCoordinates();
var (name, age, isActive) = module.GetUserRecord();

// Accessing by position
var coordinates = module.GetCoordinates();
double x = coordinates.Item1;
double y = coordinates.Item2;

// Named tuple elements (C# 7.0+)
(double X, double Y) position = module.GetCoordinates();
Console.WriteLine($"Position: ({position.X}, {position.Y})");
```

## Optional and Union Types

### Optional Types (Nullable)

| Python Type | C# Type | Notes |
|-------------|---------|-------|
| `T \| None` | `T?` | Nullable reference or value type |
| `typing.Optional[T]` | `T?` | Legacy syntax for `T \| None` |
| `typing.Union[T, None]` | `T?` | Explicit union with None |

```python
def find_user(user_id: int) -> str | None:
    users = {1: "Alice", 2: "Bob", 3: "Charlie"}
    return users.get(user_id)

def safe_divide(a: float, b: float) -> float | None:
    return a / b if b != 0 else None

def get_config_value(key: str) -> str | None:
    config = {"debug": "true", "port": "8080"}
    return config.get(key)
```

```csharp
// Handling nullable return values
string? userName = module.FindUser(1);
if (userName != null)
{
    Console.WriteLine($"Found user: {userName}");
}

// Using null-conditional operator
string display = module.FindUser(99) ?? "Unknown User";

// Pattern matching (C# 8.0+)
var result = module.SafeDivide(10.0, 0.0);
var message = result switch
{
    null => "Division by zero",
    var value => $"Result: {value}"
};
```

### Union Types (Multiple Types)

Python union types beyond nullable are mapped to `object`:

```python
def flexible_input(value: int | str | float) -> str:
    return str(value)

def get_mixed_data() -> list[int | str | bool]:
    return [1, "hello", True, 42, "world", False]
```

```csharp
// Union types become object
string result1 = module.FlexibleInput(42);        // Pass int
string result2 = module.FlexibleInput("hello");   // Pass string
string result3 = module.FlexibleInput(3.14);      // Pass double

// Mixed type collections
IReadOnlyList<object> mixedData = module.GetMixedData();
foreach (var item in mixedData)
{
    switch (item)
    {
        case long longValue:
            Console.WriteLine($"Integer: {longValue}");
            break;
        case string stringValue:
            Console.WriteLine($"String: {stringValue}");
            break;
        case bool boolValue:
            Console.WriteLine($"Boolean: {boolValue}");
            break;
    }
}
```

## Advanced Types

### Generators

| Python Type | C# Type | Notes |
|-------------|---------|-------|
| `Generator[TYield, TSend, TReturn]` | `IGeneratorIterator<TYield, TSend, TReturn>` | Full generator protocol |
| `Iterator[T]` | `IEnumerable<T>` | Simple iteration only |

```python
from typing import Generator

def count_up(start: int, end: int) -> Generator[int, None, int]:
    """Generator that yields numbers and returns count."""
    count = 0
    for i in range(start, end + 1):
        yield i
        count += 1
    return count

def interactive_generator() -> Generator[str, int, None]:
    """Generator that receives values via send()."""
    received = 0
    while received < 10:
        value = yield f"Current value: {received}"
        if value is not None:
            received = value
        else:
            received += 1
```

```csharp
// Simple iteration
var counter = module.CountUp(1, 5);
foreach (long number in counter)
{
    Console.WriteLine(number); // Prints 1, 2, 3, 4, 5
}

// Get return value after iteration
var returnValue = counter.ReturnValue; // 5 (count of yielded values)

// Interactive generator with send()
var interactive = module.InteractiveGenerator();
string first = interactive.Send(0);    // Start generator
string second = interactive.Send(5);   // Send value 5
string third = interactive.Send(8);    // Send value 8
```

### Coroutines (Async Functions)

| Python Type | C# Type | Notes |
|-------------|---------|-------|
| `Coroutine[Any, Any, T]` | `Task<T>` | Async function returning T |
| `Coroutine[Any, Any, None]` | `Task` | Async function returning void |

```python
import asyncio
from typing import Coroutine

async def fetch_data(url: str) -> dict[str, str]:
    """Simulate async HTTP request."""
    await asyncio.sleep(1)  # Simulate network delay
    return {"url": url, "status": "success", "data": "mock_data"}

async def process_batch(items: list[str]) -> None:
    """Process items asynchronously."""
    for item in items:
        await asyncio.sleep(0.1)
        print(f"Processed: {item}")
```

```csharp
// Async functions return Task<T>
IReadOnlyDictionary<string, string> data = await module.FetchDataAsync("https://api.example.com");
Console.WriteLine($"Status: {data["status"]}");

// Async void functions return Task
await module.ProcessBatchAsync(new[] { "item1", "item2", "item3" });

// Multiple async calls
var task1 = module.FetchDataAsync("url1");
var task2 = module.FetchDataAsync("url2");
var results = await Task.WhenAll(task1, task2);
```

### Buffer Protocol (NumPy Arrays)

| Python Type | C# Type | Notes |
|-------------|---------|-------|
| `typing.Buffer` | `IPyBuffer` | Generic buffer interface |
| NumPy arrays | `IPyBuffer` | Optimized for numerical data |

```python
import numpy as np
from typing import Buffer

def create_matrix() -> Buffer:
    """Create a 2D NumPy array."""
    return np.array([[1, 2, 3], [4, 5, 6]], dtype=np.float64)

def process_array(data: Buffer) -> Buffer:
    """Process numpy array and return result."""
    arr = np.asarray(data)
    return arr * 2 + 1
```

```csharp
// Working with NumPy arrays
using var matrix = module.CreateMatrix();

// Access as spans for high performance
var span = matrix.AsSpan<double>();
for (int i = 0; i < span.Length; i++)
{
    Console.WriteLine(span[i]);
}

// 2D array access
var span2D = matrix.AsSpan2D<double>();
for (int row = 0; row < span2D.Height; row++)
{
    for (int col = 0; col < span2D.Width; col++)
    {
        Console.WriteLine(span2D[row, col]);
    }
}

// Convert to managed array
double[] managedArray = matrix.ToArray<double>();
```

## Type Conversion Rules

### Implicit Conversions

These conversions happen automatically when calling Python functions:

```csharp
// C# → Python automatic conversions
module.PythonFunction(42);          // int → long → Python int
module.PythonFunction(3.14);        // double → Python float
module.PythonFunction("hello");     // string → Python str
module.PythonFunction(true);        // bool → Python bool
module.PythonFunction(new[] {1,2}); // int[] → list[int]
```

### Explicit Conversions

For complex types or when automatic conversion isn't available:

```csharp
// Manual PyObject creation
using var pyList = PyObject.From(new[] { 1, 2, 3, 4, 5 });
using var pyDict = PyObject.From(new Dictionary<string, object>
{
    ["name"] = "Alice",
    ["age"] = 30
});

// Manual conversion from PyObject
using var result = module.GetComplexResult();
var dictionary = result.As<Dictionary<string, object>>();
```

### Conversion Error Handling

```csharp
try
{
    // This might fail if Python returns incompatible type
    var stringResult = module.GetSomething().As<string>();
}
catch (InvalidCastException ex)
{
    Console.WriteLine($"Type conversion failed: {ex.Message}");
    
    // Try alternative conversion
    var objectResult = module.GetSomething().As<object>();
}

// Safe conversion with exception handling
using var result = module.GetSomething();
try
{
    var stringValue = result.As<string>();
    Console.WriteLine($"Got string: {stringValue}");
}
catch (InvalidCastException)
{
    try
    {
        var longValue = result.As<long>();
        Console.WriteLine($"Got number: {longValue}");
    }
    catch (InvalidCastException)
    {
        Console.WriteLine("Could not convert to string or number");
    }
}
```

## Custom Type Handling

### Complex Python Objects

For Python objects that don't have direct C# equivalents:

```python
class Person:
    def __init__(self, name: str, age: int):
        self.name = name
        self.age = age
    
    def to_dict(self) -> dict[str, str | int]:
        return {"name": self.name, "age": self.age}

def create_person(name: str, age: int) -> dict[str, str | int]:
    """Return person as dictionary for C# compatibility."""
    person = Person(name, age)
    return person.to_dict()
```

```csharp
// Working with complex objects as dictionaries
var personData = module.CreatePerson("Alice", 30);
string name = (string)personData["name"];
long age = (long)personData["age"];
```

### Dataclasses and NamedTuples

```python
from dataclasses import dataclass
from typing import NamedTuple

@dataclass
class Point:
    x: float
    y: float
    
    def to_tuple(self) -> tuple[float, float]:
        return (self.x, self.y)

class PointTuple(NamedTuple):
    x: float
    y: float

def create_point() -> tuple[float, float]:
    """Return point as tuple for C# compatibility."""
    point = Point(1.5, 2.5)
    return point.to_tuple()
```

```csharp
// NamedTuple/dataclass → tuple conversion
var (x, y) = module.CreatePoint();
Console.WriteLine($"Point: ({x}, {y})");
```

## Performance Considerations

### Memory Efficiency

```csharp
// ✅ Efficient - use spans for large arrays
using var buffer = module.GetLargeArray();
var span = buffer.AsSpan<double>(); // No copying
ProcessData(span);

// ❌ Inefficient - creates managed copy
using var buffer = module.GetLargeArray();
double[] array = buffer.ToArray<double>(); // Copies all data
ProcessData(array);
```

### Type Conversion Overhead

```csharp
// ✅ Efficient - direct type match
string result = module.GetString(); // Python str → C# string

// ❌ Less efficient - requires conversion
using var pyObj = module.GetComplexObject();
var dict = pyObj.As<Dictionary<string, object>>(); // PyObject → Dictionary
```

## Next Steps

- [Learn about configuration options](configuration.md)
- [Explore error handling](../user-guide/errors.md)
- [Review API reference](api.md)
