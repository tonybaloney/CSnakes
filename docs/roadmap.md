# Roadmap

This document outlines the implementation status of various Python features for CSnakes.

## Typing Constructs

Note that any constructs which are not available will be typed as `PyObject`. They still work, but the source generator won't generate the automatic marshal and unmarshal code for them. This means that you will have to manually marshal and unmarshal these types in your C# code.

| Construct (link) | Supported in CSnakes | Notes |
|------------------|----------------------|-------|
| [Any](https://docs.python.org/3/library/typing.html#typing.Any) | Yes ([Reference](reference.md#supported-types)) |  |
| [Union](https://docs.python.org/3/library/typing.html#typing.Union) | No | No equivalent construct in C#, possibly could use overloads |
| [Optional](https://docs.python.org/3/library/typing.html#typing.Optional) | Yes ([Reference](reference.md#supported-types)) |  |
| [Literal](https://docs.python.org/3/library/typing.html#typing.Literal) | No | Possible with enums |
| [Final](https://docs.python.org/3/library/typing.html#typing.Final) | No |  |
| [ClassVar](https://docs.python.org/3/library/typing.html#typing.ClassVar) |  |  |
| [Generic](https://docs.python.org/3/library/typing.html#typing.Generic) |  |  |
| [TypeVar](https://docs.python.org/3/library/typing.html#typing.TypeVar) |  |  |
| [Callable](https://docs.python.org/3/library/typing.html#typing.Callable) | No | Callables would have to by Python objects, not C# functions |
| [Tuple](https://docs.python.org/3/library/typing.html#typing.Tuple) | Yes ([Reference](reference.md#supported-types)) |  |
| [List](https://docs.python.org/3/library/typing.html#typing.List) | Yes ([Reference](reference.md#supported-types)) |  |
| [Dict](https://docs.python.org/3/library/typing.html#typing.Dict) | Yes ([Reference](reference.md#supported-types)) |  |
| [Set](https://docs.python.org/3/library/typing.html#typing.Set) |  | Usage of sets is quite niche, if you would like this feature please request it. |
| [FrozenSet](https://docs.python.org/3/library/typing.html#typing.FrozenSet) |  | As above |
| [Deque](https://docs.python.org/3/library/typing.html#typing.Deque) | No |  |
| [DefaultDict](https://docs.python.org/3/library/typing.html#typing.DefaultDict) |  |  |
| [Counter](https://docs.python.org/3/library/typing.html#typing.Counter) |  |  |
| [ChainMap](https://docs.python.org/3/library/typing.html#typing.ChainMap) |  |  |
| [Type](https://docs.python.org/3/library/typing.html#typing.Type) |  |  |
| [NewType](https://docs.python.org/3/library/typing.html#typing.NewType) |  |  |
| [NoReturn](https://docs.python.org/3/library/typing.html#typing.NoReturn) |  |  |
| [Self](https://docs.python.org/3/library/typing.html#typing.Self) |  |  |
| [Concatenate](https://docs.python.org/3/library/typing.html#typing.Concatenate) |  |  |
| [ParamSpec](https://docs.python.org/3/library/typing.html#typing.ParamSpec) |  |  |
| [Protocol](https://docs.python.org/3/library/typing.html#typing.Protocol) |  |  |
| [runtime_checkable](https://docs.python.org/3/library/typing.html#typing.runtime_checkable) |  |  |
| [Annotated](https://docs.python.org/3/library/typing.html#typing.Annotated) |  |  |
| [ForwardRef](https://docs.python.org/3/library/typing.html#typing.ForwardRef) |  |  |
| [overload](https://docs.python.org/3/library/typing.html#typing.overload) |  |  |

## Collections ABC

| Construct (link) | Supported in CSnakes | Notes |
|------------------|----------------------|-------|
| [Collection (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Collection) |  |  |
| [Container (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Container) |  |  |
| [Hashable (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Hashable) |  |  |
| [ItemsView (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.ItemsView) |  |  |
| [Iterable (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Iterable) |  |  |
| [Iterator (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Iterator) |  |  |
| [Generator (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Generator) | Yes ([Reference](reference.md#generators)) | See [Generators](reference.md#generators) for details |
| [Mapping (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Mapping) |  |  |
| [MappingView (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.MappingView) |  |  |
| [MutableMapping (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.MutableMapping) |  |  |
| [MutableSequence (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.MutableSequence) |  |  |
| [MutableSet (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.MutableSet) |  |  |
| [Reversible (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Reversible) |  |  |
| [Sequence (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Sequence) |  |  |
| [Set (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Set) |  |  |
| [Sized (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Sized) |  |  |
| [ValuesView (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.ValuesView) |  |  |
| [Awaitable (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Awaitable) |  |  |
| [Coroutine (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Coroutine) | Yes ([Reference](reference.md#supported-types)) | See [Async Support](async_support.md) |
| [AsyncIterable (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.AsyncIterable) | No |  |
| [AsyncIterator (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.AsyncIterator) | No |  |