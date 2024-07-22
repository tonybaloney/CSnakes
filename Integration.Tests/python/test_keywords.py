def test_keyword_only(a: str, *, b: int) -> None:
    print(a, b)

def test_named_keyword_only(a: str, *args, b: int = 0) -> None:
    print(a, b)
