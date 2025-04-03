import sys


def test_sys_executable() -> str:
    return sys.executable


def test_sys_path() -> str:
    return str(sys.path)
