import datetime


def test_date_roundtrip(value: datetime.date) -> datetime.date:
    return value


def test_create_date(year: int, month: int, day: int) -> datetime.date:
    return datetime.date(year, month, day)
