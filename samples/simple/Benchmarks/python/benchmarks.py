from datetime import date, datetime, time, timedelta, timezone
from uuid import uuid4, UUID

SECONDS_PER_MINUTE = 60
SECONDS_PER_HOUR = 60 * SECONDS_PER_MINUTE
SECONDS_PER_DAY = 24 * SECONDS_PER_HOUR
MICROSECONDS_PER_SECOND = 1_000_000

def python_uuid_str(val: str) -> None:
    UUID(hex=val)
    return None

def python_uuid_big_endian(val: bytes) -> None:
    UUID(bytes==val)
    return None

def python_uuid_little_endian(val: bytes) -> None:
    UUID(bytes_le=val)
    return None

def net_guid_str() -> str:
    return str(uuid4())

def net_guid_big_endian() -> bytes:
    return uuid4().bytes

def net_guid_little_endian() -> bytes:
    return uuid4().bytes_le

def python_date_str(val: str) -> None:
    date.fromisoformat(val)
    return None

def python_date_ordinal(val: int) -> None:
    date.fromordinal(val - 1)
    return None

def net_date_str() -> str:
    return date.today().isoformat()

def net_date_ordinal() -> int:
    return date.today().toordinal() + 1

def python_time_str(val: str) -> None:
    time.fromisoformat(val)
    return None

def python_time_microseconds(val: int) -> None:
    (datetime.min + timedelta(microseconds=val)).time()
    return None

def net_time_str() -> str:
    return datetime.now().time().isoformat()

def net_time_microseconds() -> int:
    val = datetime.now().time()
    return (val.hour * SECONDS_PER_HOUR + val.minute * SECONDS_PER_MINUTE + val.second) * MICROSECONDS_PER_SECOND + val.microsecond

def python_time_delta_str(val: str) -> None:
    day, hour, minute, second = val.split(':')
    second, microsecond = second.split('.')
    timedelta(int(day), int(second), int(microsecond[:6]), minutes=int(minute), hours=int(hour))
    return None

def python_time_delta_microseconds(val: int) -> None:
    timedelta(microseconds=val)
    return None

def net_time_delta_str() -> str:
    val = timedelta(microseconds=101_234_543_018_125_549) # Use gigantic value
    hours, remainder = divmod(val.seconds, SECONDS_PER_HOUR)
    minutes, seconds = divmod(remainder, SECONDS_PER_MINUTE)
    return f'{val.days}:{str(hours).zfill(2)}:{str(minutes).zfill(2)}:{str(seconds).zfill(2)}.{str(val.microseconds).zfill(6)}'

def net_time_delta_microseconds() -> int:
    val = timedelta(microseconds=101_234_543_018_125_549) # Use gigantic value
    return (val.days * SECONDS_PER_DAY + val.seconds) * MICROSECONDS_PER_SECOND + val.microseconds

def python_date_time_str(val: str) -> None:
    datetime.fromisoformat(val)
    return None

def python_date_time_offset(val: tuple[int,int]) -> None:
    days, microseconds = divmod(val[0], SECONDS_PER_DAY * MICROSECONDS_PER_SECOND)
    dto = datetime.fromordinal(days + 1) + timedelta(microseconds=microseconds)
    datetime(dto.year, dto.month, dto.day, dto.hour, dto.minute, dto.second, dto.microsecond, timezone(timedelta(seconds=val[1])))
    return None

def net_date_time_str() -> str:
    return datetime.now().isoformat()

def net_date_time_offset() -> tuple[int,int]:
    val = datetime.now()
    offset = val.utcoffset()
    # If offset is None, use the timezone offset
    if (offset is None):
        offset = val.astimezone().utcoffset()
    return ((val.toordinal() - 1) * SECONDS_PER_DAY + val.hour * SECONDS_PER_HOUR + val.minute * SECONDS_PER_MINUTE + val.second) * MICROSECONDS_PER_SECOND + val.microsecond, int(offset.total_seconds())
