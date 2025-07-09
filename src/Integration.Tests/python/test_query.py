from dataclasses import dataclass

@dataclass
class FooBarBaz:
    foo: int
    bar: str | None
    baz: list[int]
    qux: tuple[int, str]
    quux: dict[str, int]

def foo_bar_baz() -> "__extern__.Integration.Tests.QueryTests.FooBarBaz":
    return FooBarBaz(1, "hello", [1, 2, 3], (42, "world"), { "foo": 1, "bar": 2, "baz": 3 })

def foo_bar_baz_list() -> list["__extern__.Integration.Tests.QueryTests.FooBarBaz"]:
    return [foo_bar_baz()]
