try:
    from collections.abc import Buffer
except ImportError:
    from typing_extensions import Buffer

import numpy as np


def test_simple_buffer() -> Buffer:
    return np.array([1, 2, 3, 4, 5], dtype=np.int32)
