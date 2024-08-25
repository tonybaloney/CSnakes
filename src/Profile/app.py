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

from typing import Any

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


if __name__ == "__main__":
    # Start timer
    import time
    start = time.time()

    for i in range(100_000):
        generate_data(i, "hello", (3.2, "testinput"), (i % 1 == 0))

    # End timer and time ms taken
    print("Time taken: ", (time.time() - start) * 1000, "ms")
