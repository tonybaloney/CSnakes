from asyncio import Future
from typing import Any, overload, AnyStr, Optional

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

# This is a real overload from asyncio.graph module. Causes CS0111 without proper handling.
@overload
def test_same_types_but_different_defaults(future: None = None, /, *, depth: int = 1, limit: Optional[int] = None) -> None:
   ...

@overload
def test_same_types_but_different_defaults(future: Future[Any], /, *, depth: int = 1, limit: Optional[int] = None) -> None:
   pass

def test_same_types_but_different_defaults(future: Optional[Future[Any]] = None, /, *, depth: int = 1, limit: Optional[int] = None) -> None:
    if future is not None:
        future.set_result(None)
    return None
