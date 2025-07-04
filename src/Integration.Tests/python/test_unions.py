from typing import Union

def test_union_basic(a: Union[int, str, bytes]) -> None:
    pass


def test_union_return() -> Union[int, str, bytes]:
    return 42


def test_multiple_unions(a: Union[int, bool], b: Union[int, str]) -> None:
    pass

class A:
    ...

class B:
    ...

def test_multiple_complex(a: Union[A, B]) -> None:
    pass

