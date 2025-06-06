from typing import Any

def test_default_str_arg(a: str = "hello") -> str:
    return a

def test_default_int_arg(a: int = 1337) -> int:
    return a

def test_default_float_arg(a: float = -1) -> float:
    return a

def test_int_literals(a: int = 0x1337, b: int = 0b10101011) -> int:
    return a

def test_optional_int(a: int | None = None) -> bool:
    return a is None


def test_optional_str(a: str | None = None) -> bool:
    return a is None


def test_optional_list(a: list[int] | None = None) -> bool:
    return a is None


def test_optional_any(a: Any | None = None) -> bool:
    return a is None
