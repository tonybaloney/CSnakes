# Roadmap

This document outlines the implementation status of various Python features for CSnakes.

## Typing Constructs

Note that any constructs which are not available will be typed as `PyObject`. They still work, but the source generator won't generate the automatic marshal and unmarshal code for them. This means that you will have to manually marshal and unmarshal these types in your C# code.

| Construct (link) | Supported in CSnakes | Notes | Summary |
|------------------|----------------------|-------|---------|
| [Any](https://docs.python.org/3/library/typing.html#typing.Any) | Yes |  | Special type that matches any type. |
| [Union](https://docs.python.org/3/library/typing.html#typing.Union) | Yes |  | Generates overloads. |
| [Optional](https://docs.python.org/3/library/typing.html#typing.Optional) | Yes |  | Shorthand for a type or None. |
| [Literal](https://docs.python.org/3/library/typing.html#typing.Literal) | No | Possible with enums | Restricts a value to specific literal values. |
| [Final](https://docs.python.org/3/library/typing.html#typing.Final) | No | Would be simple to implement, but not used on function signatures | Indicates a name cannot be reassigned. |
| [ClassVar](https://docs.python.org/3/library/typing.html#typing.ClassVar) | No | Would be simple, but not used on function signatures | Marks a variable as a class variable. |
| [Generic](https://docs.python.org/3/library/typing.html#typing.Generic) | No | Relies on class reflection | Base class for generic types. |
| [TypeVar](https://docs.python.org/3/library/typing.html#typing.TypeVar) |  |  | Defines a generic type variable. |
| [Callable](https://docs.python.org/3/library/typing.html#typing.Callable) | No | Callables would have to by Python objects, not C# functions | Represents a callable object (e.g., function). |
| [Tuple](https://docs.python.org/3/library/typing.html#typing.Tuple) | Yes |  | Fixed-length, ordered collection of types. |
| [List](https://docs.python.org/3/library/typing.html#typing.List) | Yes |  | Variable-length, ordered collection of types. |
| [Dict](https://docs.python.org/3/library/typing.html#typing.Dict) | Yes |  | Dictionary mapping keys to values. |
| [Set](https://docs.python.org/3/library/typing.html#typing.Set) |  | Usage of sets is quite niche, if you would like this feature please request it. | Unordered collection of unique elements. |
| [FrozenSet](https://docs.python.org/3/library/typing.html#typing.FrozenSet) |  | As above | Immutable set. |
| [Deque](https://docs.python.org/3/library/typing.html#typing.Deque) | No |  | Double-ended queue. |
| [DefaultDict](https://docs.python.org/3/library/typing.html#typing.DefaultDict) | No | Use Mapping type instead | Dictionary with a default value for missing keys. |
| [Counter](https://docs.python.org/3/library/typing.html#typing.Counter) | No | Use Mapping type instead | Dict subclass for counting hashable objects. |
| [ChainMap](https://docs.python.org/3/library/typing.html#typing.ChainMap) |  |  | Groups multiple dicts into a single view. |
| [Type](https://docs.python.org/3/library/typing.html#typing.Type) |  |  | Indicates a type object. |
| [NewType](https://docs.python.org/3/library/typing.html#typing.NewType) | No | Not useful for reflection | Creates distinct types for type checking. |
| [NoReturn](https://docs.python.org/3/library/typing.html#typing.NoReturn) | No | Very unlikely to be used, but could be reflected to `void` | Indicates a function never returns. |
| [Self](https://docs.python.org/3/library/typing.html#typing.Self) | No | Will be used in class reflection | Refers to the instance type in class bodies. |
| [Concatenate](https://docs.python.org/3/library/typing.html#typing.Concatenate) | No | Very hard to achieve in C# | Used for advanced typing of callable signatures. |
| [ParamSpec](https://docs.python.org/3/library/typing.html#typing.ParamSpec) |  |  | Used for typing callable parameter lists. |
| [Protocol](https://docs.python.org/3/library/typing.html#typing.Protocol) |  |  | Defines structural subtyping (static duck typing). |
| [runtime_checkable](https://docs.python.org/3/library/typing.html#typing.runtime_checkable) |  |  | Decorator to allow isinstance checks with Protocols. |
| [Annotated](https://docs.python.org/3/library/typing.html#typing.Annotated) |  |  | Adds context-specific metadata to types. |
| [ForwardRef](https://docs.python.org/3/library/typing.html#typing.ForwardRef) |  |  | Internal type for forward references. |
| [overload](https://docs.python.org/3/library/typing.html#typing.overload) | No | [Issue](https://github.com/tonybaloney/CSnakes/issues/26) | Allows function overloading for type checkers. |

## Collections ABC

| Construct (link) | Supported in CSnakes | Notes | Summary |
|------------------|----------------------|-------|---------|
| [Collection (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Collection) | No |  | Sized iterable container with __contains__. |
| [Container (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Container) | No |  | Supports membership test using __contains__. |
| [Hashable (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Hashable) | No |  | Objects with a hash value. |
| [ItemsView (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.ItemsView) | No |  | View on dictionary's items. |
| [Iterable (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Iterable) | No | [Issue](https://github.com/tonybaloney/CSnakes/issues/479) | Object capable of returning its members one at a time. |
| [Iterator (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Iterator) | No |  | Produces items from an iterable. |
| [Generator (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Generator) | Yes | See [User Guide](../user-guide/type-system.md) for details | Iterator that supports send(), throw(), and close(). |
| [Mapping (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Mapping) | Yes |  | Collection of key-value pairs. |
| [MappingView (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.MappingView) |  |  | View on dictionary's keys, values, or items. |
| [MutableMapping (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.MutableMapping) |  |  | Mapping that can be changed. |
| [MutableSequence (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.MutableSequence) | No | Use Sequence | Sequence that can be changed. |
| [MutableSet (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.MutableSet) |  |  | Set that can be changed. |
| [Reversible (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Reversible) |  |  | Supports reversed iteration. |
| [Sequence (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Sequence) | Yes |  | Ordered collection of items. |
| [Set (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Set) |  |  | Unordered collection of unique elements. |
| [Sized (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Sized) |  |  | Has a length. |
| [ValuesView (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.ValuesView) |  |  | View on dictionary's values. |
| [Awaitable (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Awaitable) | No | Unlikely to be useful | Can be used in an await expression. |
| [Coroutine (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.Coroutine) | Yes | See [Async Support](../user-guide/async.md) | Awaitable object with send(), throw(), and close(). |
| [AsyncIterable (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.AsyncIterable) | No | [Issue](https://github.com/tonybaloney/CSnakes/issues/440) | Object capable of async iteration. |
| [AsyncIterator (collections.abc)](https://docs.python.org/3/library/collections.abc.html#collections.abc.AsyncIterator) | No | [Issue](https://github.com/tonybaloney/CSnakes/issues/440) | Async iterator object. |

## Class Reflection

### Typed Dict

| Construct (link) | Supported in CSnakes | Notes | Summary |
|------------------|----------------------|-------|---------|
| [TypedDict](https://docs.python.org/3/library/typing.html#typing.TypedDict) | No |  | Dictionary with a specific set of keys, each with a specific type. |
| [NotRequired](https://docs.python.org/3/library/typing.html#typing.NotRequired) | No |  | Marks a key as not required in a TypedDict. |
| [Required](https://docs.python.org/3/library/typing.html#typing.Required) | No |  | Marks a key as required in a TypedDict. |
| [ReadOnly](https://docs.python.org/3/library/typing.html#typing.ReadOnly) | No |  | Marks a key as read-only in a TypedDict. |

### Protocols

| Construct (link) | Supported in CSnakes | Notes | Summary |
|------------------|----------------------|-------|---------|
| [Protocol](https://docs.python.org/3/library/typing.html#typing.Protocol) | No |  | Defines structural subtyping (static duck typing). |
