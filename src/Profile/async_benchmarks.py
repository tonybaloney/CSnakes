import asyncio
from typing import Any, Coroutine


async def async_sleepy() -> Coroutine[None, None, None]:
    await asyncio.sleep(0.001)
