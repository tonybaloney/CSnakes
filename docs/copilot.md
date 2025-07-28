# GitHub Copilot Prompts for CSnakes

This page contains practical prompts designed to help you solve common CSnakes development challenges using GitHub Copilot.

## Convert Python Script to CSnakes-ready Function

**Problem**: You have a standalone Python script and need to convert it into a function that CSnakes can generate C# bindings for.

**Copy this prompt into Copilot Chat:**

```
Convert this Python script into a CSnakes-compatible function with proper type annotations.

Use only these supported CSnakes type mappings:
- int → long (C#)
- float → double (C#) 
- str → string (C#)
- bool → bool (C#)
- bytes → byte[] (C#)
- list[T] → IReadOnlyList<T> (C#)
- dict[K, V] → IReadOnlyDictionary<K, V> (C#)
- tuple[T1, T2, ...] → (T1, T2, ...) (C#)
- typing.Optional[T] or T | None → T? (C#)
- typing.Buffer → IPyBuffer (C#)
- None (return only) → void (C#)

Requirements:
1. Convert the script into one or more functions
2. Add proper type annotations to all parameters and return values
3. Use default parameter values for optional arguments (int, float, str, bool only)
4. Preserve the original logic and functionality
5. Add docstrings explaining what each function does
6. Handle any global variables by making them function parameters
7. If the script uses unsupported types, suggest alternative approaches

Here's my Python script:
[PASTE YOUR SCRIPT HERE]
```

## Example Usage

### Before (Standalone Script):
```python
import json
import sys

# Global configuration
DEFAULT_CONFIG = {"timeout": 30, "retries": 3}

# Read command line arguments
if len(sys.argv) > 1:
    input_file = sys.argv[1]
else:
    input_file = "data.json"

# Process the data
with open(input_file, 'r') as f:
    data = json.load(f)

results = []
for item in data:
    if item.get('active', False):
        processed = {
            'id': item['id'],
            'name': item['name'].upper(),
            'score': item.get('score', 0) * 2
        }
        results.append(processed)

print(f"Processed {len(results)} items")
```

### After (CSnakes-Compatible):
```python
from typing import Optional

def process_data_file(
    input_file: str = "data.json",
    timeout: int = 30,
    retries: int = 3,
    score_multiplier: float = 2.0
) -> tuple[list[dict[str, str | int]], int]:
    """
    Process a JSON data file and return active items with transformed data.
    
    Args:
        input_file: Path to the JSON file to process
        timeout: Connection timeout in seconds
        retries: Number of retry attempts
        score_multiplier: Multiplier to apply to scores
        
    Returns:
        Tuple of (processed_items, count) where processed_items is a list
        of dictionaries containing id, name, and score, and count is the
        number of items processed.
    """
    import json
    
    try:
        with open(input_file, 'r') as f:
            data = json.load(f)
    except FileNotFoundError:
        return [], 0
    
    results = []
    for item in data:
        if item.get('active', False):
            processed = {
                'id': str(item['id']),  # Ensure string type
                'name': item['name'].upper(),
                'score': int(item.get('score', 0) * score_multiplier)  # Ensure int type
            }
            results.append(processed)
    
    return results, len(results)
```

**Generated C# signature:**
```csharp
public (IReadOnlyList<IReadOnlyDictionary<string, object>>, long) ProcessDataFile(
    string inputFile = "data.json", 
    long timeout = 30, 
    long retries = 3, 
    double scoreMultiplier = 2.0);
```

## Additional Tips

When using this prompt:

1. **Replace `[PASTE YOUR SCRIPT HERE]`** with your actual Python code
2. **Review the suggestions** - Copilot might suggest breaking complex scripts into multiple functions
3. **Test the generated functions** in isolation before integrating with CSnakes
4. **Consider data flow** - make sure all necessary data is passed as parameters rather than using global variables

For more information about supported types and CSnakes features, see the [Reference Documentation](reference.md).
