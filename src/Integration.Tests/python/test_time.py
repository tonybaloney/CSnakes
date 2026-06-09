import datetime


def test_time_roundtrip(value: datetime.time) -> datetime.time:
    return value


def test_create_time(hour: int, minute: int, second: int, microsecond: int) -> datetime.time:
    return datetime.time(hour, minute, second, microsecond)
