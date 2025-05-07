from datetime import date, datetime, time, timedelta, timezone
from uuid import uuid4, UUID

sample_dict = {'a': 1, 'b': 2, 'c': 3}

def return_dict() -> dict[str, int]:
    return sample_dict

def take_dict(d: dict[str, int]) -> bool:
    print(d)
    return d == sample_dict

def return_uuid() -> tuple[bytes, str]:
    val = uuid4()
    return val.bytes_le, str(val)

def take_uuid(byte_array: bytes, string: str) -> bool:
    return UUID(bytes_le = byte_array) == UUID(hex = string)

#Note: Python's ordinal date is one day ahead of the .NET's DateNumber so we either adjust in Python or C#
def return_date() -> tuple[int, str]:
    val = date.today()
    return val.toordinal() - 1, val.isoformat()

def take_date(ordinal: int, string: str) -> bool:
    return date.fromordinal(ordinal + 1) == date.fromisoformat(string)

def return_time() -> tuple[float, tuple[int, int, int, int, int], str]:
    val = datetime.now().time()
    millisecond, microsecond = divmod(val.microsecond, 1000)
    return (datetime.combine(date.min, val) - datetime.min).total_seconds(), (val.hour, val.minute, val.second, millisecond, microsecond), val.isoformat()

def take_time(total_seconds: float, ordinal: tuple[int, int, int, int, int], string: str) -> bool:
    val = time.fromisoformat(string)
    hour, minute, second, millisecond, microsecond = ordinal
    return (datetime.min + timedelta(seconds=total_seconds)).time() == val and time(hour, minute, second, millisecond * 1000 + microsecond) == val

def return_time_delta() -> tuple[float, str]:
    val = timedelta(days=1, hours=2, minutes=3, seconds=4, milliseconds=5, microseconds=6)
    hours, remainder = divmod(val.seconds, 3600)
    minutes, seconds = divmod(remainder, 60)
    return val.total_seconds(), f'{val.days}:{str(hours).zfill(2)}:{str(minutes).zfill(2)}:{str(seconds).zfill(2)}.{str(val.microseconds).zfill(6)}'

def take_time_span(total_seconds: float, string: str) -> bool:
    days, hours, minutes, seconds = string.split(':')
    seconds, microseconds = seconds.split('.')
    return timedelta(seconds= total_seconds) == timedelta(int(days), int(seconds), int(microseconds[:6]), minutes=int(minutes), hours=int(hours))

def return_date_time(use_utc: bool = True) -> str:
    return datetime.now(timezone.utc if use_utc else None).isoformat()
