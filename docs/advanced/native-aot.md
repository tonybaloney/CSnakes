# Native AOT Support

CSnakes supports Native AOT (Ahead-of-Time) compilation, which allows you to compile your C# application to native code for faster startup times and reduced memory footprint. For comprehensive information about Native AOT in .NET, see the [Microsoft Native AOT documentation](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/).

## Overview

Native AOT compilation produces a single executable file that includes the .NET runtime, eliminating the need for the .NET runtime to be installed on the target machine. This provides several benefits:

- **Faster startup times** - No JIT compilation required
- **Reduced memory footprint** - More efficient memory usage
- **Self-contained deployment** - No external dependencies
- **Improved security** - Code is pre-compiled and harder to reverse engineer

## Requirements for Native AOT

Native AOT support in CSnakes **only works with source generated bindings**. This means:

- You must use Python files marked as `AdditionalFiles` in your project
- The source generator must be enabled (which is the default)
- You cannot use the manual Python binding approach described in [Manual Python Integration](manual-integration.md)

### Why Source Generator is Required

The limitation exists because casting Python objects to .NET containers like `Tuple`, `Dictionary`, `List`, or `Coroutine` requires reflection when done dynamically, which is not supported in Native AOT compilation. The source generator solves this by generating compiled bindings and reflection code at build time without using `System.Reflection`, making the generated code AOT-ready.

## Configuring Native AOT

### Basic Configuration

To enable Native AOT in your CSnakes project, add the following property to your `.csproj` file:

```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <InvariantGlobalization>true</InvariantGlobalization>
</PropertyGroup>
```

The `InvariantGlobalization` setting is typically required for AOT compatibility, as it reduces the application's dependencies on culture-specific data. For more details on configuration options, see [Native AOT deployment overview](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/).

### Advanced Configuration

```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <InvariantGlobalization>true</InvariantGlobalization>
  
  <!-- Optional: Optimize for size -->
  <PublishTrimmed>true</PublishTrimmed>
  <TrimMode>full</TrimMode>
  
  <!-- Optional: Disable debugging symbols for smaller size -->
  <DebuggerSupport>false</DebuggerSupport>
  <EnableUnsafeUTF7Encoding>false</EnableUnsafeUTF7Encoding>
  <HttpActivityPropagationSupport>false</HttpActivityPropagationSupport>
  <MetadataUpdaterSupport>false</MetadataUpdaterSupport>
  <UseSystemResourceKeys>true</UseSystemResourceKeys>
</PropertyGroup>
```

## Complete Example Project

Here's a complete example of a CSnakes project configured for Native AOT:

### AOTConsoleApp.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <PublishAot>true</PublishAot>
    <InvariantGlobalization>true</InvariantGlobalization>
  </PropertyGroup>

  <ItemGroup>
    <AdditionalFiles Include="*.py" SourceItemType="Python">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </AdditionalFiles>
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="CSnakes.Runtime" Version="2.*-*" />
  </ItemGroup>
</Project>
```

### aot_demo.py

```python
def cool_things() -> list[str]:
    return [
        "Python",
        "C#",
    ]

def calculate_fibonacci(n: int) -> int:
    """Calculate nth Fibonacci number"""
    if n <= 1:
        return n
    return calculate_fibonacci(n - 1) + calculate_fibonacci(n - 2)

def process_data(items: list[int]) -> dict[str, int]:
    """Process a list of integers and return statistics"""
    return {
        "count": len(items),
        "sum": sum(items),
        "max": max(items) if items else 0,
        "min": min(items) if items else 0
    }
```

### Program.cs

```csharp
using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
var home = Path.Join(Environment.CurrentDirectory);
builder.Services
    .WithPython()
    .WithHome(home)
    .FromRedistributable("3.12");

var app = builder.Build();

var env = app.Services.GetRequiredService<IPythonEnvironment>();

RunQuickDemo(env);

static void RunQuickDemo(IPythonEnvironment env)
{
    var quickDemo = env.AotDemo();
    
    Console.WriteLine("=== Cool Things Demo ===");
    foreach (var thing in quickDemo.CoolThings())
    {
        Console.WriteLine(thing + " is cool!");
    }
    
    Console.WriteLine("\n=== Fibonacci Demo ===");
    for (int i = 0; i < 10; i++)
    {
        var fib = quickDemo.CalculateFibonacci(i);
        Console.WriteLine($"Fibonacci({i}) = {fib}");
    }
    
    Console.WriteLine("\n=== Data Processing Demo ===");
    var testData = new List<int> { 1, 2, 3, 4, 5, 10, 15, 20 };
    var stats = quickDemo.ProcessData(testData);
    
    Console.WriteLine($"Data: [{string.Join(", ", testData)}]");
    foreach (var kvp in stats)
    {
        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
    }
    
    Console.WriteLine();
}
```

## Publishing for Native AOT

### Basic Publishing

To publish your application with Native AOT, use the following command:

```bash
dotnet publish -c Release -r <runtime-identifier>
```

### Platform-Specific Examples

**Windows x64:**
```bash
dotnet publish -c Release -r win-x64
```

**Linux x64:**
```bash
dotnet publish -c Release -r linux-x64
```

**macOS x64:**
```bash
dotnet publish -c Release -r osx-x64
```

**macOS ARM64 (Apple Silicon):**
```bash
dotnet publish -c Release -r osx-arm64
```

### Advanced Publishing Options

```bash
# Optimize for size
dotnet publish -c Release -r win-x64 -p:PublishTrimmed=true

# Self-contained with optimizations
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

# Debug information for troubleshooting
dotnet publish -c Release -r win-x64 -p:DebuggerSupport=true
```

For more information about publishing Native AOT applications and runtime identifiers, see the [Publishing Native AOT apps guide](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/).

## Python Environment Packaging

**Important**: While `dotnet publish` builds a self-contained .NET application, that application does not contain Python, the Python virtual environment, or its dependencies. The published application will still require a Python runtime and any required packages to be available on the target system.

### Deployment Strategies

#### 1. Using CSnakes.Stage Tool

For packaging Python environments alongside your application, see the documentation on the [`CSnakes.Stage` tool](https://tonybaloney.github.io/CSnakes/docker/) which provides guidance on bundling Python environments with your .NET applications.

```bash
# Example using CSnakes.Stage
csnakes-stage --python-version 3.12 --output ./python-env
```

#### 2. Redistributable Python

Use the redistributable Python approach for consistent deployments:

```csharp
// In your AOT application, use redistributable Python
builder.Services
    .WithPython()
    .WithHome(Environment.CurrentDirectory)
    .FromRedistributable("3.12"); // This downloads Python automatically
```

**Note:** You can customize the Python cache location by setting the `CSNAKES_REDIST_CACHE` environment variable to override the default application data folder.

```csharp

#### 3. Docker Deployment

```dockerfile
# Dockerfile for AOT + Python
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Install Python
RUN apt-get update && apt-get install -y python3 python3-pip

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["AOTConsoleApp.csproj", "."]
COPY ["*.py", "."]
RUN dotnet restore "AOTConsoleApp.csproj"

# Copy source and build
COPY . .
RUN dotnet publish "AOTConsoleApp.csproj" -c Release -r linux-x64 -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["./AOTConsoleApp"]
```

## Limitations and Considerations

### 1. Source Generator Required

Native AOT only works with the source generator. Manual Python binding using the runtime API directly is not supported in AOT scenarios due to reflection limitations when dynamically casting Python objects to .NET containers.

```csharp
// ✅ Supported in AOT - uses source generator
var module = env.MyModule();
var result = module.MyFunction(42);

// ❌ Not supported in AOT - uses reflection
using (GIL.Acquire())
{
    var module = Import.ImportModule("my_module");
    var result = module.GetAttr("my_function").Call(PyObject.From(42));
    return result.As<MyType>(); // This won't work in AOT
}
```

### 2. Reflection Limitations

Native AOT has limited support for reflection, which is why the source generator approach is mandatory. For more details, see [Native AOT compatibility requirements](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/compatibility).

### 3. Python Runtime

The Python runtime itself is not compiled to native code - only your C# application is. The Python interpreter still runs in its normal mode.

### 4. Debugging

Debugging AOT-compiled applications can be more challenging than debugging JIT-compiled applications. See [Native AOT debugging](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/debugging) for more information.

```csharp
// Enable debugging support in AOT (increases size)
<PropertyGroup>
  <DebuggerSupport>true</DebuggerSupport>
</PropertyGroup>
```

### 5. Build Time

Native AOT compilation takes longer than regular compilation.

```bash
# Regular build
dotnet build  # ~5-10 seconds

# AOT build
dotnet publish -r win-x64  # ~30-60 seconds
```

### 6. File Size

While memory usage may be reduced, the resulting executable may be larger due to including the entire runtime.

```bash
# Compare sizes
ls -la bin/Release/net8.0/AOTConsoleApp.dll           # ~50KB
ls -la bin/Release/net8.0/win-x64/publish/AOTConsoleApp.exe  # ~15-30MB
```

## Sample Project

A complete working example of Native AOT with CSnakes is available in the [samples/simple/AOTConsoleApp](https://github.com/tonybaloney/CSnakes/tree/main/samples/simple/AOTConsoleApp) directory of the repository. This sample demonstrates the proper configuration and usage patterns for AOT deployment.

### Running the Sample

```bash
# Clone the repository
git clone https://github.com/tonybaloney/CSnakes.git
cd CSnakes/samples/simple/AOTConsoleApp

# Build and run normally
dotnet run

# Publish as AOT
dotnet publish -c Release -r win-x64

# Run the AOT executable
./bin/Release/net8.0/win-x64/publish/AOTConsoleApp.exe
```

## Best Practices for AOT

### 1. Design for AOT from the Start

```csharp
// Design with AOT in mind
public class AOTFriendlyDesign
{
    // Use source generator bindings
    private readonly IAotDemoGenerated _demo;
    
    public AOTFriendlyDesign(IPythonEnvironment env)
    {
        _demo = env.AotDemo(); // Generated binding
    }
    
    // Avoid reflection-based patterns
    public void ProcessData()
    {
        var result = _demo.ProcessData(new List<int> { 1, 2, 3 });
        // Work with strongly-typed result
    }
}
```

### 2. Test AOT Builds Regularly

```bash
# Include in CI/CD pipeline
dotnet publish -c Release -r linux-x64 --verbosity normal
```

### 3. Profile Performance

```csharp
public class AOTBenchmark
{
    [Benchmark]
    public void StandardBuild()
    {
        // Benchmark JIT version
    }
    
    [Benchmark]
    public void AOTBuild()
    {
        // Benchmark AOT version
    }
}
```


## Next Steps

- [Review the complete sample project](https://github.com/tonybaloney/CSnakes/tree/main/samples/simple/AOTConsoleApp)
- [Learn about deployment strategies](../user-guide/deployment.md)
- [Explore performance optimization](performance.md)
