# Common Patterns

This guide covers the most common patterns and use cases when working with CSnakes.

## Basic Function Calls

### Simple Data Processing

```python
# data_utils.py
def clean_data(items: list[str]) -> list[str]:
    """Remove empty strings and strip whitespace."""
    return [item.strip() for item in items if item.strip()]

def count_words(text: str) -> dict[str, int]:
    """Count word frequency in text."""
    words = text.lower().split()
    return {word: words.count(word) for word in set(words)}
```

```csharp
var processor = env.DataUtils();

// Clean messy data
var messy = new[] { "  hello  ", "", "world", "  " };
var clean = processor.CleanData(messy);
// Result: ["hello", "world"]

// Count words
var counts = processor.CountWords("hello world hello");
// Result: {"hello": 2, "world": 1}
```

### Mathematical Operations

```python
# math_operations.py
import math
from typing import List

def calculate_statistics(numbers: list[float]) -> dict[str, float]:
    """Calculate basic statistics for a list of numbers."""
    if not numbers:
        return {"mean": 0, "std": 0, "min": 0, "max": 0}
    
    mean = sum(numbers) / len(numbers)
    variance = sum((x - mean) ** 2 for x in numbers) / len(numbers)
    std = math.sqrt(variance)
    
    return {
        "mean": mean,
        "std": std,
        "min": min(numbers),
        "max": max(numbers)
    }

def prime_factors(n: int) -> list[int]:
    """Find prime factors of a number."""
    factors = []
    d = 2
    while d * d <= n:
        while n % d == 0:
            factors.append(d)
            n //= d
        d += 1
    if n > 1:
        factors.append(n)
    return factors
```

```csharp
var mathOps = env.MathOperations();

var numbers = new[] { 1.5, 2.8, 3.2, 1.1, 4.5, 2.3 };
var stats = mathOps.CalculateStatistics(numbers);
Console.WriteLine($"Mean: {stats["mean"]:F2}");
Console.WriteLine($"Std Dev: {stats["std"]:F2}");

var factors = mathOps.PrimeFactors(60);
// Result: [2, 2, 3, 5]
```

## Working with External Libraries

### Using NumPy and Scientific Computing

```python
# scientific_computing.py
import numpy as np
from typing import List

def matrix_multiply(a: list[list[float]], b: list[list[float]]) -> list[list[float]]:
    """Multiply two matrices using NumPy."""
    arr_a = np.array(a)
    arr_b = np.array(b)
    result = np.dot(arr_a, arr_b)
    return result.tolist()

def solve_linear_system(a: list[list[float]], b: list[float]) -> list[float]:
    """Solve linear system Ax = b."""
    arr_a = np.array(a)
    arr_b = np.array(b)
    solution = np.linalg.solve(arr_a, arr_b)
    return solution.tolist()
```

```csharp
var scientific = env.ScientificComputing();

var matrixA = new[] {
    new[] { 1.0, 2.0 },
    new[] { 3.0, 4.0 }
};
var matrixB = new[] {
    new[] { 5.0, 6.0 },
    new[] { 7.0, 8.0 }
};

var product = scientific.MatrixMultiply(matrixA, matrixB);
// Matrix multiplication result

var coefficients = new[] {
    new[] { 2.0, 1.0 },
    new[] { 1.0, 3.0 }
};
var constants = new[] { 3.0, 4.0 };
var solution = scientific.SolveLinearSystem(coefficients, constants);
```

### Machine Learning with scikit-learn

```python
# ml_models.py
from sklearn.cluster import KMeans
from sklearn.linear_model import LinearRegression
import numpy as np

def kmeans_clustering(data: list[tuple[float, float]], n_clusters: int) -> tuple[list[list[float]], list[int]]:
    """Perform K-means clustering."""
    X = np.array(data)
    kmeans = KMeans(n_clusters=n_clusters, random_state=42)
    labels = kmeans.fit_predict(X)
    centers = kmeans.cluster_centers_
    return centers.tolist(), labels.tolist()

def linear_regression(x_data: list[float], y_data: list[float]) -> tuple[float, float]:
    """Perform linear regression and return slope and intercept."""
    X = np.array(x_data).reshape(-1, 1)
    y = np.array(y_data)
    
    model = LinearRegression()
    model.fit(X, y)
    
    return float(model.coef_[0]), float(model.intercept_)
```

```csharp
var ml = env.MlModels();

// Clustering
var points = new[] {
    (1.0, 2.0), (1.5, 1.8), (5.0, 8.0), (8.0, 8.0), (1.0, 0.6), (9.0, 11.0)
};
var (centers, labels) = ml.KmeansClustering(points, 2);

// Linear regression
var xData = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
var yData = new[] { 2.0, 4.0, 6.0, 8.0, 10.0 };
var (slope, intercept) = ml.LinearRegression(xData, yData);
Console.WriteLine($"y = {slope:F2}x + {intercept:F2}");
```

## File and Data Processing

### Working with CSV Data

```python
# csv_processor.py
import csv
from io import StringIO
from typing import Dict, List

def parse_csv(csv_content: str) -> list[dict[str, str]]:
    """Parse CSV content and return list of dictionaries."""
    reader = csv.DictReader(StringIO(csv_content))
    return list(reader)

def create_csv(data: list[dict[str, str]]) -> str:
    """Create CSV content from list of dictionaries."""
    if not data:
        return ""
    
    output = StringIO()
    fieldnames = data[0].keys()
    writer = csv.DictWriter(output, fieldnames=fieldnames)
    writer.writeheader()
    writer.writerows(data)
    return output.getvalue()

def filter_csv_data(csv_content: str, column: str, value: str) -> str:
    """Filter CSV data by column value."""
    data = parse_csv(csv_content)
    filtered = [row for row in data if row.get(column) == value]
    return create_csv(filtered)
```

```csharp
var csvProcessor = env.CsvProcessor();

var csvData = "name,age,city\nAlice,30,New York\nBob,25,Los Angeles\nCharlie,35,New York";

// Parse CSV
var parsed = csvProcessor.ParseCsv(csvData);
foreach (var row in parsed)
{
    Console.WriteLine($"{row["name"]} is {row["age"]} years old");
}

// Filter data
var filtered = csvProcessor.FilterCsvData(csvData, "city", "New York");
Console.WriteLine("New York residents:");
Console.WriteLine(filtered);
```

### JSON Processing

```python
# json_processor.py
import json
from typing import Any, Dict, List

def parse_json(json_str: str) -> dict[str, Any]:
    """Parse JSON string to dictionary."""
    return json.loads(json_str)

def create_json(data: dict[str, Any]) -> str:
    """Convert dictionary to JSON string."""
    return json.dumps(data, indent=2)

def extract_values(json_str: str, path: str) -> list[Any]:
    """Extract values from JSON using dot notation path."""
    data = json.loads(json_str)
    keys = path.split('.')
    
    result = [data]
    for key in keys:
        new_result = []
        for item in result:
            if isinstance(item, dict) and key in item:
                new_result.append(item[key])
            elif isinstance(item, list):
                for sub_item in item:
                    if isinstance(sub_item, dict) and key in sub_item:
                        new_result.append(sub_item[key])
        result = new_result
    
    return result
```

```csharp
var jsonProcessor = env.JsonProcessor();

var jsonData = @"{
    ""users"": [
        {""name"": ""Alice"", ""age"": 30},
        {""name"": ""Bob"", ""age"": 25}
    ]
}";

// Parse JSON
var parsed = jsonProcessor.ParseJson(jsonData);

// Extract specific values
var names = jsonProcessor.ExtractValues(jsonData, "users.name");
// Result: ["Alice", "Bob"]
```

## Error Handling Patterns

### Graceful Error Handling

```python
# error_handler.py
from typing import Optional

def safe_divide(a: float, b: float) -> tuple[bool, float]:
    """Safely divide two numbers, return success flag and result."""
    try:
        if b == 0:
            return False, 0.0
        return True, a / b
    except Exception:
        return False, 0.0

def validate_and_process(data: str) -> tuple[bool, str, list[str]]:
    """Validate input and return success, message, and processed data."""
    errors = []
    
    if not data:
        errors.append("Data cannot be empty")
    
    if not data.isalnum():
        errors.append("Data must be alphanumeric")
    
    if len(data) < 3:
        errors.append("Data must be at least 3 characters")
    
    if errors:
        return False, "; ".join(errors), []
    
    # Process data
    processed = [data.upper(), data.lower(), data.title()]
    return True, "Success", processed
```

```csharp
var errorHandler = env.ErrorHandler();

// Safe division
var (success, result) = errorHandler.SafeDivide(10.0, 0.0);
if (success)
{
    Console.WriteLine($"Result: {result}");
}
else
{
    Console.WriteLine("Division failed");
}

// Validation
var (isValid, message, processed) = errorHandler.ValidateAndProcess("abc123");
if (isValid)
{
    Console.WriteLine($"Processed: {string.Join(", ", processed)}");
}
else
{
    Console.WriteLine($"Validation failed: {message}");
}
```

## Configuration and Environment

### Loading Configuration from Python

```python
# config_loader.py
import os
import json
from typing import Dict, Any, Optional

def load_config(config_path: str) -> dict[str, Any]:
    """Load configuration from JSON file."""
    with open(config_path, 'r') as f:
        return json.load(f)

def get_environment_config() -> dict[str, str]:
    """Get relevant environment variables."""
    return {
        key: value for key, value in os.environ.items()
        if key.startswith(('APP_', 'DB_', 'API_'))
    }

def merge_configs(default_config: dict[str, Any], 
                 user_config: dict[str, Any]) -> dict[str, Any]:
    """Merge user config over default config."""
    merged = default_config.copy()
    merged.update(user_config)
    return merged
```

```csharp
var configLoader = env.ConfigLoader();

// Load configuration
var config = configLoader.LoadConfig("appsettings.json");
var envConfig = configLoader.GetEnvironmentConfig();

// Merge configurations
var finalConfig = configLoader.MergeConfigs(config, envConfig);
```

## Performance Optimization Patterns

### Caching and Memoization

```python
# cache_utils.py
from functools import lru_cache
from typing import List

@lru_cache(maxsize=128)
def expensive_calculation(n: int) -> int:
    """Simulate expensive calculation with caching."""
    result = 0
    for i in range(n):
        result += i * i
    return result

# Cache storage for session-level caching
_cache: dict[str, Any] = {}

def cached_process_data(cache_key: str, data: list[int]) -> list[int]:
    """Process data with manual caching."""
    if cache_key in _cache:
        return _cache[cache_key]
    
    # Expensive processing
    result = [x * x for x in data if x > 0]
    _cache[cache_key] = result
    return result

def clear_cache() -> None:
    """Clear the manual cache."""
    _cache.clear()
```

```csharp
var cacheUtils = env.CacheUtils();

// Use cached calculation
var result1 = cacheUtils.ExpensiveCalculation(1000); // First call - slow
var result2 = cacheUtils.ExpensiveCalculation(1000); // Second call - fast (cached)

// Manual caching
var data = new[] { 1, 2, 3, 4, 5 };
var processed1 = cacheUtils.CachedProcessData("key1", data); // Processed
var processed2 = cacheUtils.CachedProcessData("key1", data); // From cache

// Clear cache when needed
cacheUtils.ClearCache();
```

## Next Steps

- [Explore real-world use cases](use-cases.md)
- [Check out sample projects](sample-projects.md)
- [Learn best practices](best-practices.md)
