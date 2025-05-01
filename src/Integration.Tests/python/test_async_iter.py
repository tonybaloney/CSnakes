import asyncio
import typing

def test_basic_async_iter() -> typing.AsyncIterator[int]:

    class TestAsyncIter:
        def __init__(self):
            self._i = 0

        def __aiter__(self):
            return self

        async def __anext__(self):
            if self._i < 5:
                self._i += 1
                await asyncio.sleep(0)
                return self._i
            raise StopAsyncIteration

    return TestAsyncIter()
