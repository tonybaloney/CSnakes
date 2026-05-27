import uuid


def test_uuid_roundtrip(value: uuid.UUID) -> uuid.UUID:
    return value


def test_create_uuid(hex_str: str) -> uuid.UUID:
    return uuid.UUID(hex_str)
