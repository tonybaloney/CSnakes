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
