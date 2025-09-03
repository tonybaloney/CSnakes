def positional_only_args(a: int, /, b: int, c: int) -> None:
    pass

def collect_star_args(a: int, b: int, *args) -> None:
    pass

def keyword_only_args(a: int, *, b: int, c: int) -> None:
    pass

def collect_star_star_kwargs(a: int, b: int, **kwargs) -> None:
    pass

def positional_and_kwargs(a: int, b: int, *, c: int, **kwargs) -> None:
    pass

def collect_star_args_and_keyword_only_args(a: int, b: int, *args, c: int) -> None:
    pass
