import numpy as np
from collections.abc import Buffer

def generate_2d_float32_array(rows: int, cols: int) -> Buffer:
    return np.random.rand(rows, cols).astype(np.float32)

def generate_2d_float64_array(rows: int, cols: int) -> Buffer:
    return np.random.rand(rows, cols).astype(np.float64)

def generate_2d_bfloat16_array(rows: int, cols: int) -> Buffer:
    return np.random.rand(rows, cols).astype(np.bfloat16)