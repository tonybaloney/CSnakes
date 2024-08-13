# Limitations

This is a list of known limitations. If you really want to see any of these, or other features please [raise an issue](https://github.com/tonybaloney/CSnakes/issues/new) and describe your use case.

## Type Hinting

### Variadic Generics

C# does not have a notion of `Tuple<T...>`, so the type annotation using `Tuple[*T]` in Python cannot be statically converted into a C#.NET type. See [PEP646](https://peps.python.org/pep-0646/) for more details and [25](https://github.com/tonybaloney/CSnakes/issues/25).

### Union Types

Python's type hinting supports `Union` types, which are not supported in C#.NET. This includes both `typing.Union` and the union operator described in [PEP 604](https://peps.python.org/pep-0604/).

### Optional Types

CSnakes doesn't support the `typing.Optional[T]` type annotation. Instead, you can use the `None` default value to indicate that a parameter is optional.

## Classes

CSnakes does not support source generation for custom types, this includes dataclasses and named tuple instances. 

Functions which return class instances will return a `PyObject` in C#.NET which you can use to pass into other functions. This type is a reference to the return value. 

```python
def create_person(name: str, age: int) -> Person:
    return Person(name, age)
```

CSnakes will create a method signature like this:

```csharp
public PyObject CreatePerson(string name, long age);
```

There are some public methods on the `PyObject` class that you can use to interact with the object, such as `GetAttr` and `Call`.
Any PyObject has a `ToString()` method that will return the string representation of the object, but you cannot convert the instance to a specific CLR type. 

```csharp
var person = module.CreatePerson("Alice", 42);
var name = person.GetAttr("name");
var age = person.GetAttr("age");
```

## Async functions

Python coroutines declared using the `async` syntax are not supported. If you would like this feature, please [raise an issue](https://github.com/tonybaloney/CSnakes/issues/new).