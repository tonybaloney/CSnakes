def positional_only_args(a: int, /, b: int, c: int) -> int:
    return a + b + c

def collect_star_args(a: int, b: int, *args) -> int:
    return a + b + args[0]

def collect_star_star_kwargs(a: int, b: int, **kwargs) -> int:
    return a + b + kwargs['c']

def keyword_only_args(a: int, *, b: int, c: int) -> int:
    return a + b + c
