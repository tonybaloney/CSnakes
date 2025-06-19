import numpy as np
from ml_dtypes import bfloat16
from typing_extensions import Buffer


def generate_2d_float32_array(rows: int, cols: int) -> Buffer:
    return np.random.rand(rows, cols).astype(np.float32)

def generate_2d_float32_array_as_bytes(rows: int, cols: int) -> Buffer:
    return np.random.rand(rows, cols).astype(np.float32).tobytes()

def generate_2d_float64_array(rows: int, cols: int) -> Buffer:
    return np.random.rand(rows, cols).astype(np.float64)

def generate_2d_float64_array_as_bytes(rows: int, cols: int) -> Buffer:
    return np.random.rand(rows, cols).astype(np.float64).tobytes()

def generate_2d_bfloat16_array_as_bytes(rows: int, cols: int) -> Buffer:
    return np.random.rand(rows, cols).astype(bfloat16).tobytes()
