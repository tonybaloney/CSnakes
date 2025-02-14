from typing import Generator


def example_generator(length: int) -> Generator[str, int, bool]:
    for i in range(length):
        x = yield f"Item {i}"
        if x:
            yield f"Received {x}"

    return True


def test_normal_generator() -> Generator[str, None, None]:
    yield "one"
    yield "two"


def test_generator_sequence() -> Generator[str, None, None]:
    words = ["foo", "bar", "baz"]
    for word in words:
        yield word
