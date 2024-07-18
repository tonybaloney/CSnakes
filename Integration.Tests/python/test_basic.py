def test_int_float(a: int, b: float) -> float:
    return a + b

def test_int_int(a: int, b: int) -> int:
    return a + b

def test_float_float(a: float, b: float) -> float:
    return a + b

def test_float_int(a: float, b: int) -> float:
    return a + b

def test_list_of_ints(a: list[int]) -> list[int]:
    return a

def test_two_strings(a: str, b: str) -> str:
    return a + b

def test_two_lists_of_strings(a: list[str], b: list[str]) -> list[str]:
    return a + b

def test_two_dicts(a: dict[str, int], b: dict[str, int]) -> dict[str, int]:
    return {**a, **b}
