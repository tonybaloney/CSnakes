from typing import Coroutine
import asyncio


async def test_coroutine(seconds: float = 0.1) -> Coroutine[None, None, int]:
    await asyncio.sleep(seconds)
    return 5


async def test_coroutine_raises_exception() -> Coroutine[None, None, int]:
    raise ValueError("This is a Python exception")


async def test_coroutine_returns_nothing(seconds: float = 0.1) -> Coroutine[None, None, None]:
    await asyncio.sleep(seconds)
