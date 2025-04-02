from collections import defaultdict
from typing import ItemsView, Iterator, Mapping

def test_dict_str_int(a: dict[str, int]) -> dict[str, int]:
    return a

def test_dict_str_list_int(a: dict[str, list[int]]) -> dict[str, list[int]]:
    return a

def test_dict_str_dict_int(a: dict[str, dict[str, int]]) -> dict[str, dict[str, int]]:
    return a

class MyMappingType:
    def __init__(self, a):
        self.actual_dict = dict(a)

    def __getitem__(self, key):
        return self.actual_dict[key]

    def items(self) -> ItemsView:
        return self.actual_dict.items()

    def __len__(self) -> int:
        return self.actual_dict.__len__()

    def __iter__(self) -> Iterator:
        return self.actual_dict.__iter__()


def test_mapping(a: Mapping[str, int]) -> Mapping[str, int]:
    return MyMappingType(a)
