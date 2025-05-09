from datetime import date, datetime, time, timedelta, timezone
from uuid import uuid4, UUID

SECONDS_PER_MINUTE = 60
SECONDS_PER_HOUR = 60 * SECONDS_PER_MINUTE
SECONDS_PER_DAY = 24 * SECONDS_PER_HOUR
MICROSECONDS_PER_SECOND = 1_000_000

sample_dict = {'a': 1, 'b': 2, 'c': 3}

def return_dict() -> dict[str, int]:
    return sample_dict

def take_dict(d: dict[str, int]) -> bool:
    print(d)
    return d == sample_dict

#Note: Python uuid is big-endian and .NET Guid is little-endian
def return_uuid() -> tuple[bytes, bytes, str]:
    val = uuid4()
    return val.bytes, val.bytes_le, str(val)

def take_uuid(big_endian: bytes, little_endian: bytes, uuid_string: str) -> tuple[bool, bool]:
    val = UUID(hex = uuid_string)
    return UUID(bytes = big_endian) == val, UUID(bytes_le = little_endian) == val

#Note: Python's ordinal date is one day ahead of the .NET's DateNumber so we either adjust in Python or .NET
def return_date() -> tuple[int, str]:
    val = date.today()
    return val.toordinal() - 1, val.isoformat()

def take_date(ordinal: int, iso_string: str) -> bool:
    return date.fromordinal(ordinal + 1) == date.fromisoformat(iso_string)

# Python time is in microseconds so interop at that level
def return_time() -> tuple[int, str]:
    val = datetime.now().time()
    return (val.hour * SECONDS_PER_HOUR + val.minute * SECONDS_PER_MINUTE + val.second) * MICROSECONDS_PER_SECOND + val.microsecond, val.isoformat()

def take_time(microseconds: int, iso_string: str) -> bool:
    return (datetime.min + timedelta(microseconds=microseconds)).time() == time.fromisoformat(iso_string)

# Python time is in microseconds so interop at that level
def return_time_delta() -> tuple[int, str]:
    val = timedelta(microseconds=101_234_543_018_125_549) # Use gigantic value
    hours, remainder = divmod(val.seconds, SECONDS_PER_HOUR)
    minutes, seconds = divmod(remainder, SECONDS_PER_MINUTE)
    return (val.days * SECONDS_PER_DAY + val.seconds) * MICROSECONDS_PER_SECOND + val.microseconds, f'{val.days}:{str(hours).zfill(2)}:{str(minutes).zfill(2)}:{str(seconds).zfill(2)}.{str(val.microseconds).zfill(6)}'

def take_time_span(microseconds: int, general_long: str) -> bool:
    day, hour, minute, second = general_long.split(':')
    second, microsecond = second.split('.')
    return timedelta(microseconds=microseconds) == timedelta(int(day), int(second), int(microsecond[:6]), minutes=int(minute), hours=int(hour))

# Return microseconds since 1/1/0001 and offset seconds
def return_date_time(use_utc: bool = True) -> tuple[tuple[int, int], str]:
    val = datetime.now(timezone.utc if use_utc else None)
    offset = val.utcoffset()
    # If offset is None, use the timezone offset
    if (offset is None):
        offset = val.astimezone().utcoffset()
    return (((val.toordinal() - 1) * SECONDS_PER_DAY + val.hour * SECONDS_PER_HOUR + val.minute * SECONDS_PER_MINUTE + val.second) * MICROSECONDS_PER_SECOND + val.microsecond, int(offset.total_seconds())), val.isoformat()

def take_date_time_offset(offset_tuple: tuple[int, int], iso_string: str) -> bool:
    days, microseconds = divmod(offset_tuple[0], SECONDS_PER_DAY * MICROSECONDS_PER_SECOND)
    val = datetime.fromordinal(days + 1) + timedelta(microseconds=microseconds)
    return datetime(val.year, val.month, val.day, val.hour, val.minute, val.second, val.microsecond, timezone(timedelta(seconds=offset_tuple[1]))) == datetime.fromisoformat(iso_string)

def roundtrip_date_time_iso(iso_datetime: str) -> str:
    return datetime.fromisoformat(iso_datetime).isoformat()
