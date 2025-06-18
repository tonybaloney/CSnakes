def positional_only_args(a: int, /, b: int, c: int) -> int:
    return a + b + c

def collect_star_args(a: int, b: int, *args) -> int:
    return a + b + args[0]

def keyword_only_args(a: int, *, b: int, c: int) -> int:
    return a + b + c

def collect_star_star_kwargs(a: int, b: int, **kwargs) -> int:
    return a + b + kwargs['c']

def positional_and_kwargs(a: int, b: int, *, c: int, **kwargs) -> int:
    return a + b + c + kwargs['d']

def collect_star_args_and_keyword_only_args(a: int, b: int, *args, c: int) -> int:
    return a + b + args[0] + c

# The following functions test whether the C# name for the function parameter
# is correctly generated.

def naming1(*var_arg) -> None:
    pass

def naming2(pos_arg, /, reg_arg, *var_arg) -> None:
    pass

def naming3(pos_arg, /, *var_arg, reg_arg, kw_arg, **kw_args) -> None:
    pass
