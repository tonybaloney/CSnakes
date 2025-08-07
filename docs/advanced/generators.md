# Generators

CSnakes supports [Python generators](https://docs.python.org/3/library/typing.html#typing.Generator) using the `typing.Generator` type annotation. The `typing.Generator` type annotation is used to specify a generator function that yields values of a specific type.

CSnakes will convert a Python Generator to a CLR type in the `CSnakes.Runtime.Python` namespace that implements the `IGeneratorIterator` interface.

The `IGeneratorIterator` implements both `IEnumerable<T>` and `IEnumerator<T>` interfaces, so you can use it in `foreach` loops and LINQ queries.

For example the Python function:

```python
from typing import Generator


def example_generator(length: int) -> Generator[str, int, bool]:
    for i in range(length):
        x = yield f"Item {i}"
        if x:
            yield f"Received {x}"

    return True
```

Will return a `IGeneratorIterator<string, long, bool>`. You can use this in a C# foreach loop:

```csharp
var generator = env.ExampleGenerator(5);

foreach (string item in generator)
{
	Console.WriteLine(item);
}
```

`IGeneratorIterator` also implements a `Send` method that allows you to send values back into the generator.

The type of `.Send` is the `TSend` type parameter of the `Generator` type annotation and returns `TYield`. In the example above, the `TSend` type is `long`, so you can send a `long` value back into the generator:

```csharp
var generator = env.ExampleGenerator(5);
string nextValue= generator.Send(10);
```
