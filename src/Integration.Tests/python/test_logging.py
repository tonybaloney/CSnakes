import logging


def test_log_debug() -> None:
    logger = logging.getLogger()
    logger.setLevel(logging.DEBUG)
    assert len(logger.handlers) == 1
    logging.debug("Hello world")
