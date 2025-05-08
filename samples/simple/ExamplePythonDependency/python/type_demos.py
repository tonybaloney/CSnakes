from datetime import date, datetime, time, timedelta, timezone
from uuid import uuid4, UUID

sample_dict = {'a': 1, 'b': 2, 'c': 3}

def return_dict() -> dict[str, int]:
    return sample_dict

def take_dict(d: dict[str, int]) -> bool:
    print(d)
    return d == sample_dict

#Note: Python uuid is big-endian and .NET is little-endian
def return_uuid() -> tuple[bytes, bytes, str]:
    val = uuid4()
    return val.bytes, val.bytes_le, str(val)

def take_uuid(big_endian: bytes, little_endian: bytes, uuid_string: str) -> tuple[bool, bool]:
    val = UUID(hex = uuid_string)
    return UUID(bytes = big_endian) == val, UUID(bytes_le = little_endian) == val

#Note: Python's ordinal date is one day ahead of the .NET's DateNumber so we either adjust in Python or C#
def return_date() -> tuple[int, str]:
    val = date.today()
    return val.toordinal() - 1, val.isoformat()

def take_date(ordinal: int, iso_string: str) -> bool:
    return date.fromordinal(ordinal + 1) == date.fromisoformat(iso_string)

def return_time() -> tuple[tuple[int, int], str]:
    val = datetime.now().time()
    return (val.hour * 3600 + val.minute * 60 + val.second, val.microsecond), val.isoformat()

def take_time(time_tuple: tuple[int, int], iso_string: str) -> bool:
    hour, remainder = divmod(time_tuple[0], 3600)
    minute, second = divmod(remainder, 60)
    return time(hour, minute, second, time_tuple[1]) == time.fromisoformat(iso_string)

def return_time_delta() -> tuple[tuple[int, int], str]:
    val = timedelta(seconds = 101234543018.125549) # Use gigantic value
    hours, remainder = divmod(val.seconds, 3600)
    minutes, seconds = divmod(remainder, 60)
    return (val.days * 86400 + val.seconds, val.microseconds), f'{val.days}:{str(hours).zfill(2)}:{str(minutes).zfill(2)}:{str(seconds).zfill(2)}.{str(val.microseconds).zfill(6)}'

def take_time_span(time_tuple: tuple[int, int], general_long: str) -> bool:
    days, hours, minutes, seconds = general_long.split(':')
    seconds, microseconds = seconds.split('.')
    return timedelta(seconds=time_tuple[0], microseconds=time_tuple[1]) == timedelta(int(days), int(seconds), int(microseconds[:6]), minutes=int(minutes), hours=int(hours))

def return_date_time(use_utc: bool = True) -> str:
    return datetime.now(timezone.utc if use_utc else None).isoformat()

def roundtrip_date_time_iso(iso_datetime: str) -> str:
    return datetime.fromisoformat(iso_datetime).isoformat()
