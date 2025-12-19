# Working with Large Integers

Python's `int` type is closer in structure to C#.NET's `System.Numerics.BigInteger` than to `System.Int64`. This means that when you are working with very large integers in Python, you may need to use the `BigInteger` type in C# to handle the results.

## Converting Between BigInteger and PyObject

You can use the TypeConverter class to convert between `BigInteger` and `PyObject` instances. Here's an example of how you can call a Python function that returns a very large integer:

```csharp
using CSnakes.Runtime.Python;
using System.Numerics;

const string number = "12345678987654345678764345678987654345678765";
// Something that is too big for a long (I8)
BigInteger bignumber = BigInteger.Parse(number);

using (GIL.Acquire())
{
    using PyObject? pyObj = PyObject.From(bignumber);

    // Do stuff with the integer object
    // e.g. call a function with this as an argument

    // Convert a Python int back into a BigInteger like this..
    BigInteger integer = pyObj.As<BigInteger>();
}
```

## When to Use BigInteger

Use `BigInteger` instead of standard integer types when:

- Working with numbers larger than `long.MaxValue` (9,223,372,036,854,775,807)
- Python code returns integers that might exceed 64-bit limits
- Performing mathematical operations that could result in very large numbers
- Working with cryptographic operations or high-precision calculations

## Example: Working with Large Factorials

Python can easily calculate large factorials. Here's how to handle them in C#:

**Python code (math_operations.py):**
```python
import math

def calculate_factorial(n: int) -> int:
    """Calculate factorial of n, which can be very large"""
    return math.factorial(n)

def fibonacci_large(n: int) -> int:
    """Calculate the nth Fibonacci number"""
    if n <= 1:
        return n
    
    a, b = 0, 1
    for _ in range(2, n + 1):
        a, b = b, a + b
    return b
```

**C# code:**
```csharp
using CSnakes.Runtime;
using System.Numerics;

// Get large factorial (100! is much larger than long.MaxValue)
var mathOps = env.MathOperations();
BigInteger factorial100 = mathOps.CalculateFactorial(100);
Console.WriteLine($"100! = {factorial100}");

// Get large Fibonacci number
BigInteger fib1000 = mathOps.FibonacciLarge(1000);
Console.WriteLine($"Fibonacci(1000) = {fib1000}");
```

## Performance Considerations

- `BigInteger` operations are slower than primitive integer operations
- Use regular `long` or `int` when you know the values will fit
- Consider the trade-off between precision and performance
- BigInteger allocates memory on the heap, unlike primitive integers

## Common Patterns

### Safe Conversion from Python

```csharp
public static class SafeIntegerConversion
{
    public static long ToLongOrBigInteger(PyObject pyObj, out BigInteger? bigInteger)
    {
        bigInteger = null;
        
        try
        {
            // Try to convert to long first (more efficient)
            return pyObj.As<long>();
        }
        catch (OverflowException)
        {
            // If overflow, use BigInteger
            bigInteger = pyObj.As<BigInteger>();
            return 0; // Indicate BigInteger should be used
        }
    }
}

// Usage
using var result = env.SomeFunction();
var longValue = SafeIntegerConversion.ToLongOrBigInteger(result, out var bigIntValue);

if (bigIntValue.HasValue)
{
    Console.WriteLine($"Large number: {bigIntValue.Value}");
}
else
{
    Console.WriteLine($"Regular number: {longValue}");
}
```

### Working with BigInteger in Collections

```csharp
// Python function that returns a list of large numbers
public List<BigInteger> GetLargePrimes(int count)
{
    var primes = env.MathOperations().GenerateLargePrimes(count);
    return primes.Select(p => p.As<BigInteger>()).ToList();
}
```

## Next Steps

- [Learn about free-threading mode](free-threading.md)
- [Explore manual Python integration](manual-integration.md)
- [Review performance optimization](performance.md)
