from typing import Any, Optional, Tuple

def test_int(n: Optional[int]) -> Optional[int]:
    return n

def test_str(s: Optional[str]) -> Optional[str]:
    return s

def test_any(obj: Optional[Any]) -> Optional[Any]:
    return obj

def test_optional_tuple(a: Optional[Tuple[int, str]]) -> Optional[Tuple[int, str]]:
    return a
