import sys


def test_sys_executable() -> str:
    return sys.executable


def test_sys_path() -> str:
    return str(sys.path)

def test_sys_prefix() -> str:
    return sys.exec_prefix

def test_sys_base_prefix() -> str:
    return sys.base_exec_prefix
