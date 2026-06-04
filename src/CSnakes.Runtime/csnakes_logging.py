import logging
import queue
import threading

from collections.abc import Callable, Generator, Iterator
from logging import LogRecord
from typing import Any, Union


class _Handler(logging.Handler):
    def __init__(self) -> None:
        logging.Handler.__init__(self)
        self.queue = queue.Queue[Union[LogRecord, None]](200)
        self.stats_lock = threading.Lock()
        self.drop_count = 0

    def emit(self, record: LogRecord) -> None:
        _ = self._put(record)

    def _put(self, record: Union[LogRecord, None], attempts: int = 10) -> bool:
        for _ in range(attempts):  # attempts to try enqueue the record
            try:
                self.queue.put_nowait(record)
                return True  # successfully enqueued
            except queue.Full:
                try:
                    _ = self.queue.get_nowait()  # drop the oldest record
                    self.queue.task_done()
                    with self.stats_lock:
                        self.drop_count += 1
                except queue.Empty:
                    pass
        return False  # all attempts to put failed

    def get_records(self) -> Generator[tuple[int, int, str, Union[tuple[Any, Any, Any], None]]]:
        while True:
            record = self.queue.get()
            self.queue.task_done()
            if record is None:
                break
            with self.stats_lock:
                drop_count = self.drop_count
                self.drop_count = 0
            yield (drop_count, record.levelno, record.getMessage(), record.exc_info)

    def close(self) -> None:
        _ = self._put(None)
        logging.Handler.close(self)


def monitor(name: Union[str, None] = None) -> tuple[Generator[tuple[int, int, str, Union[tuple[Any, Any, Any], None]], None, None], Callable[[], None]]:
    handler = _Handler()
    logger = logging.getLogger(name)
    logger.addHandler(handler)

    def close():
        logger.removeHandler(handler)
        handler.close()

    return (handler.get_records(), close)
