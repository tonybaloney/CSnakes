from datetime import date, datetime, time, timedelta, timezone
from uuid import uuid4, UUID

def python_uuid_str(val: str) -> None:
    UUID(hex=val)

def python_uuid_big_endian(val: bytes) -> None:
    UUID(bytes==val)

def python_uuid_little_endian(val: bytes) -> None:
    UUID(bytes_le=val)

def net_guid_str() -> str:
    return str(uuid4())

def net_guid_big_endian() -> bytes:
    return uuid4().bytes

def net_guid_little_endian() -> bytes:
    return uuid4().bytes_le

def python_date_str(val: str) -> None:
    date.fromisoformat(val)

def python_date_ordinal(val: int) -> None:
    date.fromordinal(val - 1)

def net_date_str() -> str:
    return date.today().isoformat()

def net_date_ordinal() -> int:
    return date.today().toordinal() + 1
