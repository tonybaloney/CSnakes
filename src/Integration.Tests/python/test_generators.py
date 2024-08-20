from typing import Generator


def example_generator(length: int) -> Generator[str, int, bool]:
    for i in range(length):
        x = yield f"Item {i}"
        print(f"Received {x}")

    return True
