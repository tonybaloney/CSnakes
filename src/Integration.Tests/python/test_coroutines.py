import asyncio


async def test_coroutine(seconds: float = 0.1) -> int:
    await asyncio.sleep(seconds)
    return 5


async def test_coroutine_raises_exception() -> int:
    raise ValueError("This is a Python exception")


async def test_coroutine_returns_nothing(seconds: float = 0.1) -> None:
    await asyncio.sleep(seconds)
