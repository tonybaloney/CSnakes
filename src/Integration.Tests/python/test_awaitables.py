import types
import asyncio

from collections.abc import Awaitable
from typing import Coroutine


class MyAwaitable:
    def __init__(self, seconds: float):
        self.seconds = seconds

    def __await__(self):
        yield from asyncio.sleep(self.seconds).__await__()
        return 5


def test_awaitable(seconds: float = 0.1) -> Awaitable[int]:
    return MyAwaitable(seconds)
