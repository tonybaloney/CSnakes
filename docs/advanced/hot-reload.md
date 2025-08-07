# Hot Reload Support

CSnakes supports [hot reload](https://learn.microsoft.com/visualstudio/debugger/hot-reload?view=vs-2022) of Python code in Visual Studio and supported IDEs. This means that you can make changes to your Python code within the function body and see the changes reflected in your C# code without restarting the application.

## Overview

Hot reload functionality allows for rapid development iteration by automatically reloading Python modules when their source files change. This feature is particularly useful during development when you need to quickly test changes to Python logic without restarting your entire .NET application.

## How It Works

This feature is enabled in the generated classes in CSnakes. When you make changes to the Python code, the modules are reloaded in the .NET runtime and subsequent calls to the Python code will use the new code.

The hot reload mechanism:

1. **Monitors Python files** for changes during development
2. **Automatically reloads** modified modules
3. **Updates function bindings** to use the new code
4. **Preserves application state** in your .NET application

## Enabling Hot Reload

### Visual Studio 2022

To enable Hot Reload in Visual Studio 2022, see the [official documentation](https://learn.microsoft.com/visualstudio/debugger/hot-reload?view=vs-2022).

1. **Start debugging** your application (F5 or Debug > Start Debugging)
2. **Make changes** to your Python files
3. **Save the files** - changes should be applied automatically
4. **Verify changes** by calling the updated Python functions

### VS Code

Hot reload works with VS Code when using the C# DevKit extension by following the [extra instructions](https://code.visualstudio.com/docs/csharp/debugging#_hot-reload).


## Supported Changes

Hot reload supports changes to the **function body** of Python functions:

### ✅ Supported Changes

- **Logic modifications** within function bodies
- **Adding/removing** local variables
- **Changing calculations** and algorithms
- **Modifying string literals** and constants
- **Adding/removing** print statements or logging
- **Changing loop logic** and conditionals
- **Importing additional modules** within functions

## Limitations

Beyond the C# [limitations](https://learn.microsoft.com/visualstudio/debugger/supported-code-changes-csharp?view=vs-2022), Hot Reload does not support changes to the Python code which require additional changes to the C# interface:

### ❌ Unsupported Changes

- **Removing functions** - C# code still references them
- **Changing function signatures** - Parameter types or counts
- **Changing return types** - Would break C# type expectations
- **Changing parameter types** - Would cause type conversion errors
- **Changing function names** - C# bindings use the original names
- **Changing module names** - Module import references are cached
- **Adding new functions** - Not accessible without regenerating bindings

## Next Steps

- [Learn about signal handlers](signal-handlers.md)
- [Explore Native AOT support](native-aot.md)
- [Review troubleshooting guide](troubleshooting.md)
