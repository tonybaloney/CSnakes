import pybind11_numpy_example as pne


def test_pybind11_function() -> bool:
    n = 12
    a = pne.vector_as_array(12)  # type: ignore
    assert len(a) == n
    return True
