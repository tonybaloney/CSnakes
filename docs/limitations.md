# Limitations

## Type Hinting


### Variadic Generics

C# does not have a notion of `Tuple<T...>`, so the type annotation using `Tuple[*T]` in Python cannot be statically converted into a C#.NET type. See [PEP646](https://peps.python.org/pep-0646/) for more details.

If you really want this feature, see issue [25](https://github.com/tonybaloney/CSnakes/issues/25).

## Classes

CSnakes does not support source generation for custom types, this includes dataclasses and named tuple instances. If you would really like this feature, please [raise an issue](https://github.com/tonybaloney/CSnakes/issues/new) and describe your use case.