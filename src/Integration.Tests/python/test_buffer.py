try:
    from collections.abc import Buffer
except ImportError:
    from typing_extensions import Buffer

import numpy as np


def test_int32_buffer() -> Buffer:
    return np.array([1, 2, 3, 4, 5], dtype=np.int32)

def test_int64_buffer() -> Buffer:
    return np.array([1, 2, 3, 4, 5], dtype=np.int64)

def test_uint32_buffer() -> Buffer:
    return np.array([1, 2, 3, 4, 5], dtype=np.uint32)

def test_uint64_buffer() -> Buffer:
    return np.array([1, 2, 3, 4, 5], dtype=np.uint64)

def test_float32_buffer() -> Buffer:
    return np.array([1.1, 2.2, 3.3, 4.4, 5.5], dtype=np.float32)

def test_float64_buffer() -> Buffer:
    return np.array([1.1, 2.2, 3.3, 4.4, 5.5], dtype=np.float64)

def test_vector_buffer() -> Buffer:
    # create an array with 1532 floats between 0 and 1, similar to an embedding vector
    return np.random.rand(1532).astype(np.float32)

def test_2d_int32_matrix_buffer() -> Buffer:
    return np.array([[1, 2, 3], [4, 5, 6]], dtype=np.int32)

def test_float32_matrix_buffer() -> Buffer:
    # create an array with 100 * 100 floats between 0 and 1
    return np.random.rand(100, 100).astype(np.float32)

arr = np.zeros((2, 3), dtype=np.int32)

def test_global_buffer() -> Buffer:
    return arr
