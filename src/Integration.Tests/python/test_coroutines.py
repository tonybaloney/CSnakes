from typing import Coroutine
import asyncio


async def test_coroutine() -> Coroutine[int, None, None]:
    await asyncio.sleep(0.1)
    return 5


async def test_coroutine_raises_exception() -> Coroutine[int, None, None]:
    raise ValueError("This is a Python exception")


async def test_coroutine_returns_nothing() -> Coroutine[None, None, None]:
    await asyncio.sleep(1.0)
