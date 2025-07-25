from dataclasses import dataclass
from typing import Union

@dataclass
class FooBarBaz:
    foo: int
    bar: Union[str, None]
    baz: list[int]
    qux: tuple[int, str]
    quux: dict[str, int]

def foo_bar_baz() -> "__extern__.Integration.Tests.RichReturnTests.FooBarBaz": # pyright: ignore[reportUndefinedVariable]
    return FooBarBaz(1, "hello", [1, 2, 3], (42, "world"), { "foo": 1, "bar": 2, "baz": 3 })

def foo_bar_baz_list() -> list["__extern__.Integration.Tests.RichReturnTests.FooBarBaz"]: # pyright: ignore[reportUndefinedVariable]
    return [foo_bar_baz()]

def foo_bar_baz_dict(key: str) -> dict[str, "__extern__.Integration.Tests.RichReturnTests.FooBarBaz"]: # pyright: ignore[reportUndefinedVariable]
    return { key: foo_bar_baz() }
