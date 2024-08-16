from collections import defaultdict
from typing import Mapping

def test_dict_str_int(a: dict[str, int]) -> dict[str, int]:
    return a

def test_dict_str_list_int(a: dict[str, list[int]]) -> dict[str, list[int]]:
    return a

def test_dict_str_dict_int(a: dict[str, dict[str, int]]) -> dict[str, dict[str, int]]:
    return a

def test_mapping(a: Mapping[str, int]) -> Mapping[str, int]:
    return defaultdict(a)
