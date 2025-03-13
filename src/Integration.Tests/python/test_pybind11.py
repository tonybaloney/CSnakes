import pybind11_numpy_example as pne
import numpy as np


def test_pybind11_function() -> bool:
    n = 12
    a = pne.vector_as_array(12)
    assert isinstance(a, nd.array), a
    assert len(a) == n
    assert a.dtype == np.int16
    for i in range(n):
        assert a[i] == i
    return True
