def test_str_actually_returns_int() -> str:
    return 1

def test_str_actually_returns_float() -> str:
    return 1.0

def test_str_actually_returns_list() -> str:
    return [1, 2, 3]

def test_str_actually_returns_tuple() -> str:
    return (1, 2, 3)

def test_tuple_actually_returns_int() -> tuple[int, int]:
    return 1

def test_tuple_actually_returns_float() -> tuple[float, float]:
    return 1.0

def test_tuple_actually_returns_list() -> tuple[list[int], list[int]]:
    return [1, 2, 3]

