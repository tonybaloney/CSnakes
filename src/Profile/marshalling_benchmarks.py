import asyncio
from typing import Any, Coroutine

def generate_data(a: int, b: str, c: tuple[float, str], d: bool) -> list[dict[str, tuple[int, float]]]:
    return [
        {
            b: (a, c[0]),
            "test1": (1, 2.),
            "test2": (1, 2.),
            "test3": (1, 2.),
         },
         {
            c[1]: (a, c[0]),
            "test1": (1, 2.),
            "test2": (1, 2.),
            "test3": (1, 2.),
         }
        ]


def generate_data_any(a: int, b: str, c: tuple[float, str], d: bool) -> Any:
    return [
        {
            b: (a, c[0]),
            "test1": (1, 2.),
            "test2": (1, 2.),
            "test3": (1, 2.),
         },
         {
            c[1]: (a, c[0]),
            "test1": (1, 2.),
            "test2": (1, 2.),
            "test3": (1, 2.),
         }
        ]

def empty_function() -> None:
    pass

def generate_sequence() -> list[int]:
    return [i for i in range(100)]

def consume_sequence(sequence: list[int]) -> None:
    assert(isinstance(sequence, list) and len(sequence) == 100)

def generate_sequence_any() -> Any:
    return [i for i in range(100)]

def consume_dictionary(data: dict[str, int]) -> None:
    assert(isinstance(data, dict) and len(data) == 100)

def generate_dictionary() -> dict[str, int]:
    return {str(i): i for i in range(100)}

def generate_tuple() -> tuple[int, str, float, bool]:
    return (1, "test", 3.2, True)

def consume_tuple(data: tuple[int, str, float, bool]) -> None:
    assert(isinstance(data, tuple) and len(data) == 4)

def consume_value_types(a: int, b: str, c: float, d: bool) -> None:
    assert(isinstance(a, int) and isinstance(b, str) and isinstance(c, float) and isinstance(d, bool))


if __name__ == "__main__":
    # Start timer
    import time
    start = time.time()

    for i in range(100_000):
        generate_data(i, "hello", (3.2, "testinput"), (i % 1 == 0))

    # End timer and time ms taken
    print("Time taken: ", (time.time() - start) * 1000, "ms")
