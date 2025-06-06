from typing import Any

def test_int(n: int | None) -> int | None:
    return n

def test_str(s: str | None) -> str | None:
    return s

def test_any(obj: Any | None) -> Any | None:
    return obj
