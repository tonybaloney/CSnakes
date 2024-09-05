try:
    from collections.abc import Buffer
except ImportError:
    from typing_extensions import Buffer

import numpy as np


def test_simple_buffer() -> Buffer:
    return np.array([1, 2, 3, 4, 5], dtype=np.int32)


def test_float_scalar() -> Buffer:
    # create an array with 1532 floats between 0 and 1, similar to an embedding vector
    return np.random.rand(1532).astype(np.float32)
