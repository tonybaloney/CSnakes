from typing import Iterable


class SomeIterable:
    def __init__(self, items):
        self.items = items

    def __iter__(self):
        return iter(self.items)


def test_iterable_items() -> Iterable[int]:
    return SomeIterable([1, 2, 3])


def test_builtin_iterable() -> Iterable[str]:
    return ["apple", "banana", "cherry"]
