# Configuration Reference

This reference covers all configuration options available in CSnakes for setting up and customizing Python environments.

## Environment Configuration

### Basic Configuration

```csharp
builder.Services
    .WithPython()
    .WithHome("./python_modules")           // Python module directory
    .FromRedistributable();                 // Python locator
```

### Python Locators

#### Redistributable Locator (Recommended)

Automatically downloads and caches Python runtime.

```csharp
// Default Python 3.12
.FromRedistributable()

// Specific version
.FromRedistributable("3.13")

// Debug build (macOS/Linux only)
.FromRedistributable("3.13", debug: true)

// Free-threaded build (Python 3.13+)
.FromRedistributable("3.13", debug: false, freeThreaded: true)

// Both debug and free-threaded
.FromRedistributable("3.13", debug: true, freeThreaded: true)
```

**Configuration Options:**
- `version`: Python version ("3.9", "3.10", "3.11", "3.12", "3.13")
- `debug`: Enable debug build (better for debugging C extensions)
- `freeThreaded`: Enable free-threaded mode (experimental, Python 3.13+)

**Environment Variables:**
- `CSNAKES_REDIST_CACHE`: Custom cache directory for downloaded Python

#### Environment Variable Locator

Uses environment variable to locate Python.

```csharp
// Standard environment variable
.FromEnvironmentVariable("PYTHONHOME", "3.12")

// GitHub Actions setup-python
.FromEnvironmentVariable("Python3_ROOT_DIR", "3.12")

// Custom variable
.FromEnvironmentVariable("MY_PYTHON_PATH", "3.11")
```

#### Folder Locator

Points to specific Python installation directory.

```csharp
// Windows
.FromFolder(@"C:\Python312", "3.12")

// Linux/macOS
.FromFolder("/usr/local/python3.12", "3.12")

// Relative path
.FromFolder("./python", "3.12")
```

#### Platform-Specific Locators

**Windows Store Locator:**
```csharp
.FromWindowsStore("3.12")
```

**Windows Installer Locator:**
```csharp
.FromWindowsInstaller("3.12")
```

**macOS Installer Locator:**
```csharp
.FromMacOSInstaller("3.12")
```

**NuGet Locator:**
```csharp
.FromNuGet("3.12.4")  // Requires exact version
```

#### Source Build Locator

For custom Python builds from source.

```csharp
// Basic source build
.FromSource(@"C:\path\to\cpython", "3.13")

// Debug build
.FromSource(@"C:\path\to\cpython", "3.13", debug: true)

// Free-threaded build
.FromSource(@"C:\path\to\cpython", "3.13", debug: false, freeThreaded: true)
```

#### Conda Locator

For Anaconda/Miniconda environments.

```csharp
// Default conda installation
.FromConda("base")

// Specific conda environment
.FromConda("myenv")

// Custom conda path
.FromConda(@"C:\path\to\conda", "myenv")
```

## Virtual Environment Configuration

### Basic Virtual Environment

```csharp
builder.Services
    .WithPython()
    .WithHome("./python_modules")
    .WithVirtualEnvironment("./venv")       // Virtual environment path
    .FromRedistributable();
```

### Automatic Package Installation

```csharp
builder.Services
    .WithPython()
    .WithHome("./python_modules")
    .WithVirtualEnvironment("./venv")
    .WithPipInstaller()                     // Auto-install requirements.txt
    .FromRedistributable();
```

### Conda Environment

```csharp
builder.Services
    .WithPython()
    .WithHome("./python_modules")
    .WithCondaEnvironment("myenv")          // Conda environment name
    .FromConda();
```

## Advanced Configuration Options

### Custom Environment Variables

```csharp
var environmentBuilder = builder.Services
    .WithPython()
    .WithHome("./python_modules");

// Set custom environment variables
Environment.SetEnvironmentVariable("PYTHONPATH", "/custom/path");
Environment.SetEnvironmentVariable("PYTHONVERBOSE", "1");

environmentBuilder.FromRedistributable();
```

### Configuration via Options Pattern

```csharp
// Configuration class
public class PythonConfiguration
{
    public string PythonHome { get; set; } = "./python_modules";
    public string VirtualEnvironmentPath { get; set; } = "./venv";
    public string PythonVersion { get; set; } = "3.12";
    public bool UsePipInstaller { get; set; } = true;
    public bool EnableDebugging { get; set; } = false;
    public bool EnableFreeThreading { get; set; } = false;
    public Dictionary<string, string> EnvironmentVariables { get; set; } = new();
}

// Registration
builder.Services.Configure<PythonConfiguration>(
    builder.Configuration.GetSection("Python"));

builder.Services.AddSingleton<IPythonEnvironment>(serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IOptions<PythonConfiguration>>().Value;
    var logger = serviceProvider.GetRequiredService<ILogger<IPythonEnvironment>>();
    
    // Set environment variables
    foreach (var kvp in config.EnvironmentVariables)
    {
        Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
    }
    
    var pythonBuilder = serviceProvider
        .WithPython()
        .WithHome(config.PythonHome);
    
    if (!string.IsNullOrEmpty(config.VirtualEnvironmentPath))
    {
        pythonBuilder.WithVirtualEnvironment(config.VirtualEnvironmentPath);
    }
    
    if (config.UsePipInstaller)
    {
        pythonBuilder.WithPipInstaller();
    }
    
    return pythonBuilder.FromRedistributable(
        config.PythonVersion,
        debug: config.EnableDebugging,
        freeThreaded: config.EnableFreeThreading);
});
```

### Configuration via appsettings.json

```json
{
  "Python": {
    "PythonHome": "./python_modules",
    "VirtualEnvironmentPath": "./venv",
    "PythonVersion": "3.12",
    "UsePipInstaller": true,
    "EnableDebugging": false,
    "EnableFreeThreading": false,
    "EnvironmentVariables": {
      "PYTHONPATH": "/additional/path",
      "PYTHONVERBOSE": "0",
      "OMP_NUM_THREADS": "4"
    }
  },
  "Logging": {
    "LogLevel": {
      "CSnakes": "Information"
    }
  }
}
```

### Environment-Specific Configuration

```json
// appsettings.Development.json
{
  "Python": {
    "EnableDebugging": true,
    "EnvironmentVariables": {
      "PYTHONVERBOSE": "1",
      "PYTHONDEBUG": "1"
    }
  },
  "Logging": {
    "LogLevel": {
      "CSnakes": "Debug"
    }
  }
}

// appsettings.Production.json
{
  "Python": {
    "EnableDebugging": false,
    "EnvironmentVariables": {
      "PYTHONOPTIMIZE": "2"
    }
  },
  "Logging": {
    "LogLevel": {
      "CSnakes": "Warning"
    }
  }
}
```

## Project File Configuration

### Basic Python File Inclusion

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CSnakes.Runtime" Version="0.16.0" />
  </ItemGroup>

  <!-- Include Python files for source generation -->
  <ItemGroup>
    <AdditionalFiles Include="python_modules/**/*.py">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </AdditionalFiles>
  </ItemGroup>

  <!-- Include requirements.txt -->
  <ItemGroup>
    <Content Include="requirements.txt">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>
  </ItemGroup>
</Project>
```

### Advanced Project Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    
    <!-- Enable AOT if needed -->
    <PublishAot Condition="'$(Configuration)' == 'Release'">true</PublishAot>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CSnakes.Runtime" Version="0.16.0" />
  </ItemGroup>

  <!-- Python source files -->
  <ItemGroup>
    <AdditionalFiles Include="python_modules/**/*.py" Exclude="python_modules/tests/**">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </AdditionalFiles>
  </ItemGroup>

  <!-- Python requirements -->
  <ItemGroup>
    <Content Include="requirements.txt">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>
    <Content Include="requirements-dev.txt">
      <CopyToOutputDirectory>Never</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

  <!-- Additional Python files -->
  <ItemGroup>
    <Content Include="python_modules/**/*.json" Exclude="python_modules/tests/**">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>
    <Content Include="python_modules/**/*.yaml" Exclude="python_modules/tests/**">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

  <!-- Conditional inclusion for different environments -->
  <ItemGroup Condition="'$(Configuration)' == 'Debug'">
    <AdditionalFiles Include="python_modules/debug/**/*.py">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </AdditionalFiles>
  </ItemGroup>
</Project>
```

## Logging Configuration

### Basic Logging Setup

```csharp
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// CSnakes-specific logging
builder.Logging.AddFilter("CSnakes", LogLevel.Debug);
```

### Detailed Logging Configuration

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      
      // CSnakes logging
      "CSnakes.Runtime": "Information",
      "CSnakes.SourceGeneration": "Warning",
      
      // Your application
      "MyApp.Services.PythonService": "Debug"
    },
    "Console": {
      "IncludeScopes": true,
      "TimestampFormat": "yyyy-MM-dd HH:mm:ss "
    }
  }
}
```

### Custom Logging Provider

```csharp
public class PythonLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new PythonLogger(categoryName);
    }

    public void Dispose() { }
}

public class PythonLogger : ILogger
{
    private readonly string _categoryName;

    public PythonLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, 
        Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var message = formatter(state, exception);
        
        // Custom logging logic for Python-related events
        if (_categoryName.StartsWith("CSnakes"))
        {
            // Special handling for CSnakes logs
            Console.WriteLine($"[PYTHON] {logLevel}: {message}");
        }
        else
        {
            Console.WriteLine($"[{logLevel}] {_categoryName}: {message}");
        }

        if (exception != null)
        {
            Console.WriteLine($"Exception: {exception}");
        }
    }
}

// Registration
builder.Logging.AddProvider(new PythonLoggerProvider());
```

## Performance Configuration

### Memory Management

```csharp
// Configure GC for Python interop
public static class PythonMemoryConfiguration
{
    public static void ConfigureForPython()
    {
        // Set GC mode for better Python interop
        GCSettings.LatencyMode = GCLatencyMode.Batch;
        
        // Increase generation 0 collection threshold
        // (Python objects may have short lifetimes)
        // Note: These are example values, tune for your application
    }
}

// Call during startup
PythonMemoryConfiguration.ConfigureForPython();
```

### Thread Pool Configuration

```csharp
// Configure thread pool for Python operations
ThreadPool.SetMinThreads(
    workerThreads: Environment.ProcessorCount * 2,
    completionPortThreads: Environment.ProcessorCount
);

ThreadPool.SetMaxThreads(
    workerThreads: Environment.ProcessorCount * 4,
    completionPortThreads: Environment.ProcessorCount * 2
);
```

## Security Configuration

### Restricted Execution

```csharp
// Limit Python module access
var restrictedBuilder = builder.Services
    .WithPython()
    .WithHome("./safe_python_modules")  // Only trusted modules
    .WithVirtualEnvironment("./restricted_venv");

// Set security environment variables
Environment.SetEnvironmentVariable("PYTHONPATH", "");  // Clear Python path
Environment.SetEnvironmentVariable("PYTHONDONTWRITEBYTECODE", "1");  // No .pyc files
Environment.SetEnvironmentVariable("PYTHONNOUSERSITE", "1");  // No user site packages

restrictedBuilder.FromRedistributable();
```

### Input Validation Configuration

```csharp
public class SecurePythonService
{
    private readonly IPythonEnvironment _python;
    private readonly PythonSecurityOptions _options;

    public SecurePythonService(IPythonEnvironment python, IOptions<PythonSecurityOptions> options)
    {
        _python = python;
        _options = options.Value;
    }

    public string ProcessSecurely(string input)
    {
        // Validate input according to security policy
        if (input.Length > _options.MaxInputLength)
            throw new SecurityException("Input too long");
            
        if (_options.ForbiddenPatterns.Any(pattern => input.Contains(pattern)))
            throw new SecurityException("Input contains forbidden pattern");

        var processor = _python.SecureProcessor();
        return processor.SafeProcess(input);
    }
}

public class PythonSecurityOptions
{
    public int MaxInputLength { get; set; } = 1000;
    public List<string> ForbiddenPatterns { get; set; } = new() { "__import__", "eval", "exec" };
    public bool AllowFileAccess { get; set; } = false;
    public bool AllowNetworkAccess { get; set; } = false;
}
```

## Testing Configuration

### Test Environment Setup

```csharp
// Test-specific configuration
public class TestPythonEnvironment
{
    public static IPythonEnvironment Create()
    {
        var services = new ServiceCollection();
        
        services
            .WithPython()
            .WithHome("./test_python_modules")
            .WithVirtualEnvironment("./test_venv")
            .FromRedistributable("3.12");
            
        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider.GetRequiredService<IPythonEnvironment>();
    }
}

[TestClass]
public class PythonIntegrationTests
{
    private IPythonEnvironment _python;

    [TestInitialize]
    public void Setup()
    {
        _python = TestPythonEnvironment.Create();
    }

    [TestCleanup]
    public void Cleanup()
    {
        (_python as IDisposable)?.Dispose();
    }
}
```

### Mock Configuration for Unit Tests

```csharp
public interface IPythonEnvironmentFactory
{
    IPythonEnvironment CreateEnvironment();
}

public class ProductionPythonEnvironmentFactory : IPythonEnvironmentFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ProductionPythonEnvironmentFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPythonEnvironment CreateEnvironment()
    {
        return _serviceProvider.GetRequiredService<IPythonEnvironment>();
    }
}

public class MockPythonEnvironmentFactory : IPythonEnvironmentFactory
{
    public IPythonEnvironment CreateEnvironment()
    {
        return new MockPythonEnvironment();
    }
}

// Registration
#if DEBUG
builder.Services.AddSingleton<IPythonEnvironmentFactory, MockPythonEnvironmentFactory>();
#else
builder.Services.AddSingleton<IPythonEnvironmentFactory, ProductionPythonEnvironmentFactory>();
#endif
```

## Container Configuration

### Docker Configuration

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Install Python
RUN apt-get update && apt-get install -y \
    python3 \
    python3-pip \
    python3-venv \
    && rm -rf /var/lib/apt/lists/*

# Set Python path
ENV PYTHONPATH=/app/python_modules
ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1

WORKDIR /app

# Copy Python requirements and install
COPY requirements.txt ./
RUN python3 -m pip install --no-cache-dir -r requirements.txt

# Copy application
COPY --from=build /app/out ./
COPY python_modules ./python_modules/

# Configure for container
ENV CSnakes__PythonHome=/app/python_modules
ENV CSnakes__UsePipInstaller=false

ENTRYPOINT ["dotnet", "MyApp.dll"]
```

### Kubernetes Configuration

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: csnakes-app
spec:
  replicas: 3
  selector:
    matchLabels:
      app: csnakes-app
  template:
    metadata:
      labels:
        app: csnakes-app
    spec:
      containers:
      - name: app
        image: myapp:latest
        env:
        - name: Python__PythonHome
          value: "/app/python_modules"
        - name: Python__EnableDebugging
          value: "false"
        - name: Python__PythonVersion
          value: "3.12"
        resources:
          limits:
            memory: "512Mi"
            cpu: "500m"
          requests:
            memory: "256Mi"
            cpu: "250m"
```

## Next Steps

- [Learn about error handling](../user-guide/errors.md)
- [Review API reference](api.md)
- [Explore type mapping](type-mapping.md)
