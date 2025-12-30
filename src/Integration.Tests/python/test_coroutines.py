import types
import asyncio

from typing import Coroutine


async def test_coroutine(seconds: float = 0.1) -> int:
    await asyncio.sleep(seconds)
    return 5


async def test_coroutine_raises_exception() -> int:
    raise ValueError("This is a Python exception")


async def test_coroutine_returns_nothing(seconds: float = 0.1) -> None:
    await asyncio.sleep(seconds)


def test_coroutine_bare(seconds: float = 0.1) -> Coroutine[None, None, int]:
    return test_coroutine(seconds)


async def test_coroutine_self_canceling():
    task = asyncio.current_task()
    assert task
    _ = task.cancel()
    return 42


@types.coroutine
def test_generator_based_coroutine(seconds: float = 0.1):
    return (yield from test_coroutine(seconds))
