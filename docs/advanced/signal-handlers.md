# Signal Handler Configuration

By default, Python will install signal handlers for certain signals, such as `SIGINT` (Ctrl+C) and `SIGTERM`. This can interfere with the normal operation of your application, especially if you are using a framework that has its own signal handlers.

When Python handles these signals instead of your .NET application, it can prevent proper shutdown procedures and interrupt normal signal processing in your C# code.

## Understanding the Issue

### Default Python Behavior

Python automatically installs signal handlers for:

- **SIGINT** (Ctrl+C) - Keyboard interrupt
- **SIGTERM** - Termination request
- **SIGPIPE** - Broken pipe
- **Other signals** depending on the platform

This means that signal handlers on C# code will not be called when the signal is received, and the Python code will handle the signal instead.

### Impact on .NET Applications

```csharp
// This C# signal handler might not work as expected
Console.CancelKeyPress += (sender, e) =>
{
    Console.WriteLine("C# handling Ctrl+C");
    // This might not be called if Python handles SIGINT first
    e.Cancel = true;
};
```

## Disabling Python Signal Handlers

You can disable this behavior by using the `.DisableSignalHandlers()` method on the Python environment configuration:

```csharp
var builder = Host.CreateApplicationBuilder();
var pb = builder.Services.WithPython()
  .WithHome(Environment.CurrentDirectory)
  .FromRedistributable()
  .DisableSignalHandlers(); // Disable Python signal handlers
var app = builder.Build();

env = app.Services.GetRequiredService<IPythonEnvironment>();
```

## Use Cases for Disabling Signal Handlers

### Web Applications

In ASP.NET Core applications, you typically want the framework to handle shutdown signals:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Disable Python signal handlers to let ASP.NET handle them
builder.Services.WithPython()
    .WithHome(Environment.CurrentDirectory)
    .FromRedistributable()
    .DisableSignalHandlers();

var app = builder.Build();

// ASP.NET Core handles shutdown gracefully
app.Lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("Application is shutting down...");
    // Cleanup code here
});
```

### Console Applications with Custom Signal Handling

```csharp
class Program
{
    private static bool isShuttingDown = false;
    
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.WithPython()
            .WithHome(Environment.CurrentDirectory)
            .FromRedistributable()
            .DisableSignalHandlers(); // Let C# handle signals
        
        var app = builder.Build();
        
        // Install C# signal handlers
        Console.CancelKeyPress += OnCancelKeyPress;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        
        var env = app.Services.GetRequiredService<IPythonEnvironment>();
        
        // Your application logic
        await RunApplication(env);
    }
    
    private static void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        Console.WriteLine("Received Ctrl+C, shutting down gracefully...");
        e.Cancel = true; // Prevent immediate termination
        isShuttingDown = true;
    }
    
    private static void OnProcessExit(object sender, EventArgs e)
    {
        Console.WriteLine("Process exiting, cleaning up...");
        // Cleanup code
    }
    
    private static async Task RunApplication(IPythonEnvironment env)
    {
        while (!isShuttingDown)
        {
            // Your application work
            await Task.Delay(1000);
        }
        
        Console.WriteLine("Application shutdown complete.");
    }
}
```

### Windows Services

Windows services need precise control over signal handling:

```csharp
public class PythonWorkerService : BackgroundService
{
    private readonly IPythonEnvironment _python;
    private readonly ILogger<PythonWorkerService> _logger;
    
    public PythonWorkerService(IPythonEnvironment python, ILogger<PythonWorkerService> logger)
    {
        _python = python;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Python Worker Service starting");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Your Python work here
                var processor = _python.DataProcessor();
                await processor.ProcessBatchAsync();
                
                await Task.Delay(5000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Operation cancelled, shutting down");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Python worker service");
            }
        }
        
        _logger.LogInformation("Python Worker Service stopped");
    }
}

// Service registration
var builder = Host.CreateApplicationBuilder(args);
builder.Services.WithPython()
    .WithHome(Environment.CurrentDirectory)
    .FromRedistributable()
    .DisableSignalHandlers(); // Critical for Windows services

builder.Services.AddHostedService<PythonWorkerService>();
```

## When to Keep Python Signal Handlers

### Python-Centric Applications

If your application is primarily Python code with minimal .NET logic:

```csharp
// Keep Python signal handlers for Python-heavy applications
var builder = Host.CreateApplicationBuilder();
builder.Services.WithPython()
    .WithHome(Environment.CurrentDirectory)
    .FromRedistributable();
    // Don't call DisableSignalHandlers()

var app = builder.Build();
var env = app.Services.GetRequiredService<IPythonEnvironment>();

// Let Python handle everything
var pythonApp = env.MainApplication();
pythonApp.Run(); // Python controls the entire application lifecycle
```

### Jupyter-style Applications

For interactive or notebook-style applications where Python should handle interrupts:

```python
# interactive_processor.py
import signal
import sys

def setup_signal_handlers():
    """Setup Python signal handlers for interactive use"""
    def signal_handler(sig, frame):
        print(f'\nReceived signal {sig}, cleaning up...')
        # Python cleanup code
        sys.exit(0)
    
    signal.signal(signal.SIGINT, signal_handler)
    signal.signal(signal.SIGTERM, signal_handler)

def interactive_loop():
    """Interactive processing loop"""
    setup_signal_handlers()
    
    while True:
        try:
            user_input = input("Enter command (or 'quit'): ")
            if user_input.lower() == 'quit':
                break
            
            # Process command
            print(f"Processing: {user_input}")
            
        except KeyboardInterrupt:
            print("\nUse 'quit' to exit properly")
            continue
```

```csharp
// Let Python handle the interactive session
var interactive = env.InteractiveProcessor();
interactive.InteractiveLoop(); // Python manages the session
```

## Platform-Specific Considerations

### Windows

On Windows, signal handling is limited compared to Unix systems:

```csharp
// Windows-specific signal handling
var builder = Host.CreateApplicationBuilder();

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    // Windows has limited signal support
    builder.Services.WithPython()
        .WithHome(Environment.CurrentDirectory)
        .FromRedistributable()
        .DisableSignalHandlers(); // Usually better on Windows
}
else
{
    // Unix systems have richer signal support
    builder.Services.WithPython()
        .WithHome(Environment.CurrentDirectory)
        .FromRedistributable();
    // May keep Python handlers for Unix signals
}
```

### Linux/macOS

Unix systems support more signal types:

```csharp
// Unix-specific considerations
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || 
    RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    // Consider keeping Python handlers for SIGUSR1, SIGUSR2, etc.
    // but disable for SIGINT, SIGTERM if you need .NET control
    
    builder.Services.WithPython()
        .WithHome(Environment.CurrentDirectory)
        .FromRedistributable()
        .DisableSignalHandlers(); // Still recommended for .NET apps
}
```

## Testing Signal Handling

### Test Signal Handler Behavior

```csharp
public class SignalHandlerTest
{
    private bool signalReceived = false;
    
    public async Task TestSignalHandling()
    {
        // Setup C# signal handler
        Console.CancelKeyPress += (sender, e) =>
        {
            signalReceived = true;
            e.Cancel = true;
        };
        
        Console.WriteLine("Press Ctrl+C to test signal handling...");
        
        // Wait for signal
        while (!signalReceived)
        {
            await Task.Delay(100);
        }
        
        Console.WriteLine("Signal received by C# handler!");
    }
}
```

### Python Signal Testing Module

```python
# signal_test.py
import signal
import time

def test_signal_handling() -> dict[str, str]:
    """Test which process handles signals"""
    results = {}
    
    # Check if Python signal handlers are installed
    current_sigint = signal.signal(signal.SIGINT, signal.SIG_IGN)
    current_sigterm = signal.signal(signal.SIGTERM, signal.SIG_IGN)
    
    # Restore original handlers
    signal.signal(signal.SIGINT, current_sigint)
    signal.signal(signal.SIGTERM, current_sigterm)
    
    results["sigint_handler"] = str(current_sigint)
    results["sigterm_handler"] = str(current_sigterm)
    results["test_timestamp"] = str(time.time())
    
    return results
```

```csharp
// Test signal configuration
var signalTest = env.SignalTest();
var results = signalTest.TestSignalHandling();

Console.WriteLine($"SIGINT handler: {results["sigint_handler"]}");
Console.WriteLine($"SIGTERM handler: {results["sigterm_handler"]}");
```

## Best Practices

### 1. Default to Disabling Python Signal Handlers

For most .NET applications, disable Python signal handlers:

```csharp
// Recommended for most .NET applications
builder.Services.WithPython()
    .WithHome(Environment.CurrentDirectory)
    .FromRedistributable()
    .DisableSignalHandlers();
```

### 2. Document Signal Handling Strategy

Be explicit about your signal handling approach:

```csharp
/// <summary>
/// Configures Python environment with disabled signal handlers.
/// This allows the .NET application to handle SIGINT/SIGTERM properly.
/// </summary>
public static class PythonConfiguration
{
    public static void ConfigureForDotNetApplication(IServiceCollection services)
    {
        services.WithPython()
            .WithHome(Environment.CurrentDirectory)
            .FromRedistributable()
            .DisableSignalHandlers(); // Critical for proper .NET signal handling
    }
}
```

### 3. Test Signal Behavior

Always test signal handling in your specific environment:

```csharp
public class SignalHandlingIntegrationTest
{
    [Test]
    public async Task TestGracefulShutdown()
    {
        // Setup application with disabled Python signal handlers
        var cts = new CancellationTokenSource();
        
        var task = RunApplicationAsync(cts.Token);
        
        // Simulate signal
        cts.Cancel();
        
        // Verify graceful shutdown
        await task;
        Assert.IsTrue(task.IsCompletedSuccessfully);
    }
}
```

### 4. Monitor Signal Handling in Production

```csharp
public class SignalHandlingMonitor
{
    private readonly ILogger _logger;
    
    public SignalHandlingMonitor(ILogger<SignalHandlingMonitor> logger)
    {
        _logger = logger;
        
        Console.CancelKeyPress += OnCancelKeyPress;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }
    
    private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
    {
        _logger.LogInformation("SIGINT received, initiating graceful shutdown");
        // Signal handling logic
    }
    
    private void OnProcessExit(object sender, EventArgs e)
    {
        _logger.LogInformation("Process exit signal received");
        // Cleanup logic
    }
}
```

## Troubleshooting

### Signal Handlers Not Working

1. **Check if Python handlers are disabled**:
   ```csharp
   // Ensure this is called
   .DisableSignalHandlers()
   ```

2. **Verify signal handler installation**:
   ```csharp
   Console.CancelKeyPress += (s, e) => 
   {
       Console.WriteLine("C# handler called"); // Should see this
   };
   ```

3. **Test with minimal example**:
   ```csharp
   // Minimal test without Python
   Console.CancelKeyPress += (s, e) => Console.WriteLine("Works!");
   Console.ReadLine();
   ```

### Python Code Expects Signal Handlers

If Python code expects to handle signals:

```python
# Modify Python code to be signal-agnostic
def setup_optional_signal_handlers():
    """Setup signal handlers only if not disabled"""
    try:
        import signal
        signal.signal(signal.SIGINT, custom_handler)
    except (AttributeError, OSError):
        # Signal handling disabled or not available
        print("Signal handlers not available, using alternative approach")
        setup_alternative_shutdown_mechanism()
```

## Next Steps

- [Learn about Native AOT support](native-aot.md)
- [Explore development workflows](development.md)
- [Review troubleshooting guide](troubleshooting.md)
