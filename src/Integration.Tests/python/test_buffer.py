try:
    from collections.abc import Buffer
except ImportError:
    from typing_extensions import Buffer

import numpy as np

def test_bool_buffer() -> Buffer:
    return np.array([True, False, True, False, False], dtype=np.bool_)

def test_int8_buffer() -> Buffer:
    return np.array([1, 2, 3, 4, 5], dtype=np.int8)

def test_uint8_buffer() -> Buffer:
    return np.array([1, 2, 3, 4, 5], dtype=np.uint8)

def test_int16_buffer() -> Buffer:
    return np.array([1, 2, 3, 4, 5], dtype=np.int16)

def test_uint16_buffer() -> Buffer:
    return np.array([1, 2, 3, 4, 5], dtype=np.uint16)

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

def test_int8_2d_buffer() -> Buffer:
    return np.array([[1, 2, 3], [4, 5, 6]], dtype=np.int8)

def test_uint8_2d_buffer() -> Buffer:
    return np.array([[1, 2, 3], [4, 5, 6]], dtype=np.uint8)

def test_int16_2d_buffer() -> Buffer:
    return np.array([[1, 2, 3], [4, 5, 6]], dtype=np.int16)

def test_uint16_2d_buffer() -> Buffer:
    return np.array([[1, 2, 3], [4, 5, 6]], dtype=np.uint16)

def test_int32_2d_buffer() -> Buffer:
    return np.array([[1, 2, 3], [4, 5, 6]], dtype=np.int32)

def test_uint32_2d_buffer() -> Buffer:
    return np.array([[1, 2, 3], [4, 5, 6]], dtype=np.uint32)

def test_int64_2d_buffer() -> Buffer:
    return np.array([[1, 2, 3], [4, 5, 6]], dtype=np.int64)

def test_uint64_2d_buffer() -> Buffer:
    return np.array([[1, 2, 3], [4, 5, 6]], dtype=np.uint64)

def test_float32_2d_buffer() -> Buffer:
    return np.array([[1.1 , 2.2, 3.3], [4.4, 5.5, 6.6]], dtype=np.float32)

def test_float64_2d_buffer() -> Buffer:
    return np.array([[1.1 , 2.2, 3.3], [4.4, 5.5, 6.6]], dtype=np.float64)

arr = np.zeros((2, 3), dtype=np.int32)

def test_global_buffer() -> Buffer:
    return arr

def test_bytes_as_buffer() -> Buffer:
    return b"hello"

def test_bytearray_as_buffer() -> Buffer:
    return bytearray(b"hello")

def test_non_buffer() -> Buffer:
    return "hello"

def test_non_contiguous_buffer() -> Buffer:
    return np.array([[1, 2, 3], [4, 5, 6]], dtype=np.int32)[::2]

def test_transposed_buffer() -> Buffer:
    return np.array([[1, 2, 3], [4, 5, 6]], dtype=np.int32).T
