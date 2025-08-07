# Sample Projects

This section provides links and descriptions of complete sample projects that demonstrate CSnakes in action.

## Available Sample Projects

### 1. Basic Console Application

**Location**: [`samples/simple/QuickConsoleTest`](https://github.com/tonybaloney/CSnakes/tree/main/samples/simple/QuickConsoleTest)

**Description**: The simplest possible CSnakes implementation showing basic Python function calls from C#.

**Features**:
- Basic string manipulation functions
- Mathematical operations  
- Type conversion examples
- Error handling demonstration

**Key Files**:
- `hello_world.py` - Simple greeting function with optional parameters
- `Program.cs` - Basic C# console application setup

**What You'll Learn**:
- Basic CSnakes project setup
- Simple function calls
- Parameter passing
- Return value handling

### 2. AOT Console Application

**Location**: [`samples/simple/AOTConsoleApp`](https://github.com/tonybaloney/CSnakes/tree/main/samples/simple/AOTConsoleApp)

**Description**: Demonstrates CSnakes compatibility with Native AOT compilation for improved performance.

**Features**:
- Native AOT compilation support
- Minimal memory footprint
- Fast startup times
- Source generator usage (required for AOT)

**Key Files**:
- `aot_demo.py` - Simple Python functions for AOT demo
- `Program.cs` - AOT-compatible C# code
- `AOTConsoleApp.csproj` - Project configuration with `<PublishAot>true</PublishAot>`

**What You'll Learn**:
- AOT compatibility requirements
- Performance benefits of AOT
- Source generator usage
- Deployment considerations

### 3. Web Application

**Location**: [`samples/simple/WebApp`](https://github.com/tonybaloney/CSnakes/tree/main/samples/simple/WebApp)

**Description**: ASP.NET Core web application integrating Python data processing capabilities.

**Features**:
- REST API endpoints calling Python functions
- Dependency injection integration
- Machine learning model integration
- Performance monitoring
- Load testing configuration

**Key Files**:
- `kmeans_example.py` - K-means clustering implementation
- `phi3_demo.py` - Small language model integration
- `Controllers/` - Web API controllers
- `loadtest.jmx` - JMeter load test configuration

**What You'll Learn**:
- Web application integration
- Dependency injection setup
- API design patterns
- Performance considerations
- Load testing strategies

### 4. F# Sample

**Location**: [`samples/simple/FSharpSample`](https://github.com/tonybaloney/CSnakes/tree/main/samples/simple/FSharpSample)

**Description**: Demonstrates CSnakes usage in F# functional programming context.

**Features**:
- F# functional programming patterns
- Immutable data structures
- Pipeline operations
- Type-safe Python integration

**Key Files**:
- `math_functions.py` - Mathematical operations
- `Program.fs` - F# application using CSnakes
- `FSharpSample.fsproj` - F# project configuration

**What You'll Learn**:
- F# and CSnakes integration
- Functional programming patterns
- Type safety in F#
- Cross-language interoperability

### 5. Aspire Distributed Application

**Location**: [`samples/Aspire`](https://github.com/tonybaloney/CSnakes/tree/main/samples/Aspire)

**Description**: .NET Aspire orchestrated application with multiple services using CSnakes.

**Features**:
- Service orchestration with .NET Aspire
- Multiple microservices
- Shared Python modules
- Service discovery
- Monitoring and observability

**Key Files**:
- `CSnakesAspire.AppHost/` - Aspire orchestration host
- `CSnakesAspire.ApiService/` - API service with CSnakes
- `CSnakesAspire.Web/` - Web frontend
- `python/` - Shared Python modules

**What You'll Learn**:
- Microservices architecture
- .NET Aspire orchestration
- Service communication
- Distributed system patterns
- Monitoring and logging

## Sample Project Details

### Machine Learning Examples

#### K-means Clustering

**File**: `kmeans_example.py`

```python
from sklearn.cluster import k_means
import numpy as np

def calculate_kmeans_inertia(data: list[tuple[int, int]], n_clusters: int) -> tuple[list[list[float]], float]:
    X = np.array(data)
    centroid, label, inertia = k_means(
        X, n_clusters=n_clusters, n_init="auto", random_state=0
    )
    return centroid.tolist(), inertia
```

**C# Usage**:
```csharp
var kmeans = env.KmeansExample();
var data = new[] { (1, 2), (1, 4), (1, 0), (10, 2), (10, 4), (10, 0) };
var (centroids, inertia) = kmeans.CalculateKmeansInertia(data, 2);
```

#### Language Model Integration

**File**: `phi3_demo.py`

Demonstrates integration with Hugging Face transformers and PyTorch for running small language models directly in .NET applications.

**Requirements**:
- `transformers` package
- `torch` package
- Sufficient system memory for model loading

### Data Processing Examples

#### CSV Processing

Shows how to process CSV data using pandas and return structured results to C#:

```python
def analyze_sales_data(csv_content: str) -> dict[str, float]:
    df = pd.read_csv(StringIO(csv_content))
    return {
        "total_sales": float(df['sales'].sum()),
        "average_sale": float(df['sales'].mean()),
        "max_sale": float(df['sales'].max())
    }
```

#### JSON Manipulation

Demonstrates complex JSON processing and transformation:

```python
def transform_user_data(users: list[dict[str, any]]) -> dict[str, list[str]]:
    by_department = {}
    for user in users:
        dept = user.get('department', 'unknown')
        if dept not in by_department:
            by_department[dept] = []
        by_department[dept].append(user['name'])
    return by_department
```

## Getting Started with Samples

### 1. Clone the Repository

```bash
git clone https://github.com/tonybaloney/CSnakes.git
cd CSnakes/samples
```

### 2. Choose a Sample

Navigate to the sample you want to explore:

```bash
cd simple/QuickConsoleTest
```

### 3. Install Dependencies

For samples requiring Python packages:

```bash
# Create virtual environment
python -m venv .venv
.venv\Scripts\activate  # Windows
source .venv/bin/activate  # Linux/macOS

# Install requirements
pip install -r requirements.txt
```

### 4. Run the Sample

```bash
dotnet run
```

## Sample Project Structure

Most samples follow this structure:

```
SampleProject/
├── SampleProject.csproj          # Project configuration
├── Program.cs                    # Main application entry point
├── python_modules/               # Python code directory
│   ├── __init__.py              # Makes it a Python package
│   ├── module1.py               # Python functions
│   └── module2.py               # More Python functions
├── requirements.txt             # Python dependencies
├── README.md                    # Sample-specific documentation
└── appsettings.json            # Configuration (for web apps)
```

## Performance Benchmarks

### Web Application Performance

The WebApp sample includes JMeter load test configurations showing:

- **Throughput**: 1000+ requests/second for simple operations
- **Latency**: Sub-millisecond Python function calls
- **Memory**: Stable memory usage under load
- **Scalability**: Linear scaling with CPU cores

### Startup Performance

Comparison of startup times:

| Configuration | Cold Start | Warm Start |
|--------------|------------|------------|
| Standard Runtime | ~200ms | ~50ms |
| AOT Compiled | ~50ms | ~10ms |
| With Virtual Env | ~300ms | ~75ms |

## Troubleshooting Samples

### Common Issues

1. **Python Not Found**
   - Solution: Use `FromRedistributable()` locator
   - Alternative: Set `PYTHONHOME` environment variable

2. **Package Import Errors**
   - Solution: Check `requirements.txt` and virtual environment setup
   - Alternative: Use `WithPipInstaller()` method

3. **Build Errors**
   - Solution: Ensure Python files are marked as `AdditionalFiles`
   - Alternative: Check .csproj configuration

4. **Runtime Exceptions**
   - Solution: Check Python syntax and type annotations
   - Alternative: Use try-catch blocks around Python calls

### Getting Help

- Check the [FAQ](../community/faq.md) for common questions
- Review [troubleshooting guide](../advanced/troubleshooting.md) for detailed solutions
- Examine the sample's README.md for specific instructions

## Next Steps

- [Explore advanced usage patterns](../advanced/advanced-usage.md)
- [Understand performance optimization](../advanced/performance.md)
