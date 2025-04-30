import asyncio
from typing import Any, Coroutine


async def async_sleepy(delay: float = 0.001) -> Coroutine[None, None, None]:
    await asyncio.sleep(delay)
