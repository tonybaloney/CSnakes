def positional_only_args(a: int, /, b: int, c: int) -> int:
    return a + b + c

def keyword_only_args(a: int, *, b: int, c: int) -> int:
    return a + b + c
