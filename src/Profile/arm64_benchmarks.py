"""
Arm64-specific performance benchmarks for Windows Arm64 Python interop.
These benchmarks validate performance characteristics and compare Arm64 vs x64 behavior.
"""

import platform
import math
import time
import os
import hashlib
import base64
import datetime


def detect_architecture() -> str:
    """Benchmark Arm64 architecture detection performance"""
    return platform.machine()


def get_platform_info() -> str:
    """Benchmark platform information retrieval on Arm64"""
    return f"{platform.platform()}-{platform.processor()}"


def math_operations_arm64() -> float:
    """Test mathematical operations performance on Arm64"""
    result = 0.0
    for i in range(100):
        result += math.sqrt(i * 2.5)
        result *= math.sin(i * 0.1)
        result += math.cos(i * 0.2)
    return result


def large_data_marshalling() -> int:
    """Benchmark large data marshalling on Arm64"""
    large_list = [i * 2 for i in range(10000)]
    return len(large_list)


def string_processing_arm64() -> int:
    """Test string processing performance on Arm64"""
    test_string = "Arm64 performance testing with CSnakes Python interop framework"
    total_length = 0
    for i in range(1000):
        upper = test_string.upper()
        lower = test_string.lower()
        replaced = test_string.replace("Arm64", "ARM")
        # Use all transformations to ensure they're not optimized away
        total_length += len(upper) + len(lower) + len(replaced)
    return total_length


def environment_access() -> str:
    """Benchmark environment variable access on Arm64"""
    path = os.environ.get("PATH", "notfound")
    processor = os.environ.get("PROCESSOR_ARCHITECTURE", "unknown")
    return f"{len(path)}-{processor}"


def native_module_performance() -> str:
    """Test native module import performance (using stdlib modules)"""
    try:
        # Test basic cryptographic operations available in stdlib
        data = b"Arm64 performance test data"
        hash_result = hashlib.sha256(data).hexdigest()
        b64_result = base64.b64encode(data).decode()
        return f"crypto-{len(hash_result)}-{len(b64_result)}"
    except Exception as e:
        return f"crypto-error-{str(e)}"


def function_call_overhead() -> float:
    """Benchmark Python function call overhead on Arm64"""
    result = 0.0
    for i in range(1000):
        result += math.sqrt(25.0)
    return result


def complex_object_manipulation() -> int:
    """Test complex object creation and manipulation on Arm64"""
    data_dict = {}
    for i in range(100):
        data_dict[str(i)] = {
            "id": i,
            "name": f"item_{i}",
            "values": [i * 2, i * 3, i * 4],
            "metadata": {
                "architecture": "Arm64",
                "framework": "CSnakes",
                "timestamp": datetime.datetime.utcnow().timestamp()
            }
        }
    return len(data_dict)


def exception_handling_performance() -> int:
    """Test exception handling performance on Arm64"""
    success_count = 0
    for i in range(100):
        try:
            if i % 10 == 0:
                int("not_a_number")
            else:
                int(str(i))
                success_count += 1
        except ValueError:
            pass
    return success_count


def arm64_native_performance() -> str:
    """Test Arm64-specific optimizations and native performance"""
    # Performance test with native operations that might benefit from Arm64 optimizations
    start = time.perf_counter()
    for i in range(1000):
        # Math operations that might benefit from Arm64 optimizations
        x = i * 3.14159
        y = x ** 2.5
        z = y / 1.618
        final = int(z) % 1000
    end = time.perf_counter()

    return f"arm64-perf-{end-start:.6f}-{final}"


def runtime_architecture_detection() -> str:
    """Helper function for runtime architecture detection"""
    return platform.machine()
