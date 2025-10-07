import logging

logger = logging.getLogger()
logger.setLevel(logging.DEBUG)

def test_log_debug() -> None:
    assert len(logger.handlers) == 1
    logger.debug("Hello world")

def test_log_info() -> None:
    assert len(logger.handlers) == 1
    logger.info("Hello info world")

def test_params_message() -> None:
    assert len(logger.handlers) == 1
    logger.warning("Hello %s example %d", "this", 3)

def test_log_exception() -> None:
    try:
        1 / 0  # Example of an exception
    except Exception:
        logging.exception("An error message occurred")


def test_many_entries(count: int) -> None:
    for i in range(0, count):
        logger.warning("Error %d", i)


def test_named_logger(name: str) -> None:
    l = logging.getLogger(name)
    l.debug("Specific log entry")
