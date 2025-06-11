from typing import Any, overload, AnyStr

StrType = str

@overload
def test_overload_supported_type(x: int) -> int:
    ...

@overload
def test_overload_supported_type(x: float) -> float:
    ...

def test_overload_supported_type(x: Any) -> Any:
    return x

@overload
def test_overload_unsupported_type(x: StrType) -> str:
    ...

@overload
def test_overload_unsupported_type(x: Any) -> str:
    ...

def test_overload_unsupported_type(x: AnyStr) -> str:
    return str(x) + "!"
