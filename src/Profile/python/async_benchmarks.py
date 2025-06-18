import asyncio


async def async_sleepy(delay: float = 0.001) -> None:
    await asyncio.sleep(delay)
