some_global = 1337

def test_raise_python_exception() -> str:
    a = 1
    b = 2
    raise ValueError("This is a Python exception")


def test_nested_python_exception() -> str:
    a = 1
    b = 2
    try:
        raise ValueError("This is a Python exception")
    except ValueError as e:
        raise ValueError("This is a nested Python exception") from e
