import httpx

def test_nothing() -> bool:
    return True


def test_attrs_function()-> bool:
    from attrs import define
    @define
    class TestClass:
        name: str = "test"
    instance = TestClass()
    return instance.name == "test"
