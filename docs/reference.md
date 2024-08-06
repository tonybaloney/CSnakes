# Reference

## Supported Types

CSnakes supports the following typed scenarios:

| Python type annotation | Reflected C# Type |
|------------------------|-------------------|
| `int`                  | `long`            |
| `float`                | `double`          |
| `str`                  | `string`          |
| `bool`                 | `bool`            |
| `list[T]`              | `IEnumerable<T>`  |
| `dict[K, V]`           | `IReadOnlyDictionary<K, V>` |
| `tuple[T1, T2, ...]`   | `(T1, T2, ...)`   |

### Return types

The same type conversion applies for the return type of the Python function, with the additional feature that functions which explicitly return type `None` are declared as `void` in C#.

### Default values

CSnakes will use the default value for arguments of types `int`, `float`, `str`, and `bool` for the generated C# method. For example, the following Python code:

```python
def example(a: int = 123, b: bool = True, c: str = "hello", d: float = 1.23) -> None
  ...

```

Will generate the following C#.NET method signature:

```csharp
public void Example(long a = 123, bool b = true, string c = "hello", double d = 1.23)
```

1. CSnakes will treat `=None` default values as nullable arguments. The Python runtime will set the value of the parameter to the `None` value at execution.
