from typing import Any, Optional

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

def many_keyword_only_args(
    *,
    pipeline: Any,
    messages: list[dict[str, str]],
    max_length: Optional[int] = None,
    max_new_tokens: Optional[int] = None,
    min_length: Optional[int] = None,
    min_new_tokens: Optional[int] = None,
    stop_strings: Optional[list[str]] = None,
    temperature: Optional[float] = 1.0,
    top_k: Optional[int] = 50,
    top_p: Optional[float] = 1.0,
    min_p: Optional[float] = None,
) -> list[dict[str, str]]:
    """
    Signature inspired from TransformersSharp [1].

        [1]: https://github.com/tonybaloney/TransformersSharp/blob/2181cf6e4f4d45822b7191f1fadd0df633084d83/TransformersSharp/python/transformers_wrapper.py#L21-L32
    """
    return []
