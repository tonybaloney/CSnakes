# Example Solutions

## Most Basic Example

In the sample solution, the file [`hello_world.py`](https://github.com/tonybaloney/CSnakes/blob/main/samples/simple/ExamplePythonDependency/hello_world.py) demonstrates the most basic example of embedding Python code into a .NET project.

The Python file contains a single function `hello_world` that takes a single argument `name` and returns a greeting message. It takes an optional `int` of the maximum length of the greeting message, which defaults to 50.

```python
def format_name(name: str, max_length: int = 50) -> str:
	return "Hello {}".format(name.capitalize())[:max_length]
```

This demo generates the following C# method signature:

```csharp
public string FormatName(string name, long maxLength = 50);
```

## Native AOT Console Application

The [`AOTConsoleApp`](https://github.com/tonybaloney/CSnakes/tree/main/samples/simple/AOTConsoleApp) sample demonstrates how to use CSnakes with Native AOT (Ahead-of-Time) compilation for improved startup performance and reduced memory footprint.

This sample shows:
- Proper project configuration for Native AOT with the `<PublishAot>true</PublishAot>` setting  
- Source generator usage (required for AOT support)
- Simple Python function calls in an AOT-compiled application

The Python file [`aot_demo.py`](https://github.com/tonybaloney/CSnakes/blob/main/samples/simple/AOTConsoleApp/aot_demo.py) contains:

```python
def cool_things() -> list[str]:
    return [
        "Python",
        "C#",
    ]
```

**Important**: Native AOT support in CSnakes only works with source generated bindings. Manual Python binding approaches are not compatible with AOT compilation. See the [Native AOT Support](advanced.md#native-aot-support) section in the Advanced Usage guide for complete details.

## KMeans Example

A more complex example is the [`kmeans_example.py`](https://github.com/tonybaloney/CSnakes/blob/main/samples/simple/ExamplePythonDependency/kmeans_example.py) file, which contains a function `calculate_kmeans_inertia` that calculates the inertia of a KMeans clustering algorithm using the `sklearn` library:

```python
from sklearn.cluster import k_means
import numpy as np

def calculate_kmeans_inertia(data: list[tuple[int, int]], n_clusters: int) -> tuple[list[list[float]], float]:
    # Demo data
    X = np.array(data)
    centroid, label, inertia = k_means(
        X, n_clusters=n_clusters, n_init="auto", random_state=0
    )
    return centroid.tolist(), inertia
```

Because the input and output types are more complex, we've used a list of tuple of `int`, `int` the data input to represent the data matrix. The output is the list of centroids (the numpy array converted to a list) and the inertia value.

This demo generates the following C# method signature:

```csharp
public (IReadOnlyList<IReadOnlyList<double>>, double) CalculateKmeansInertia(IReadOnlyList<(long, long)> data, long nClusters);
```

## Phi-3 inference demo

The [`phi3_demo.py`](https://github.com/tonybaloney/CSnakes/blob/main/samples/simple/ExamplePythonDependency/phi3_demo.py) file contains a demo of using the `transformers` package from hugging face and pytorch to invoke a Small Language Model (Phi3) and complete an input string.

This demo requires the `transformers` and `torch` packages to be installed in the Python environment. PyTorch has special requirements and should be installed per the instructions on the [PyTorch website](https://pytorch.org/get-started/locally/).

## Embedding Python in a .NET Web App

The [`webapp`](https://github.com/tonybaloney/CSnakes/blob/main/samples/simple/WebApp) project demonstrates how to embed Python code in a .NET web application. The `webapp` project is a simple web application that exposes some of the example Python functions via HTTP routes.

This demo also comes with a [JMeter load test.](https://github.com/tonybaloney/CSnakes/blob/main/samples/simple/WebApp/loadtest.jmx)