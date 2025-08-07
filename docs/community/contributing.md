# Contributing to CSnakes

We welcome contributions to CSnakes! This guide outlines how to contribute to the project, from reporting bugs to submitting code changes.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Types of Contributions](#types-of-contributions)
- [Development Setup](#development-setup)
- [Contribution Workflow](#contribution-workflow)
- [Code Standards](#code-standards)
- [Testing Guidelines](#testing-guidelines)
- [Documentation](#documentation)
- [Release Process](#release-process)

## Code of Conduct

By participating in this project, you agree to abide by our Code of Conduct. We are committed to providing a welcoming and inclusive environment for all contributors.

### Our Standards

- **Be respectful**: Treat everyone with respect and courtesy
- **Be inclusive**: Welcome newcomers and help them get started
- **Be constructive**: Provide helpful feedback and suggestions
- **Be patient**: Remember that people have different experience levels
- **Be collaborative**: Work together to improve the project

## Getting Started

### Prerequisites

Before contributing, ensure you have:

- **.NET 8.0 SDK** or later
- **Python 3.8+** installed
- **Git** for version control
- **Visual Studio 2022** or **VS Code** (recommended IDEs)

### First-time Setup

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/yourusername/CSnakes.git
   cd CSnakes
   ```
3. **Add upstream remote**:
   ```bash
   git remote add upstream https://github.com/tonybaloney/CSnakes.git
   ```
4. **Install dependencies**:
   ```bash
   dotnet restore
   ```

## Types of Contributions

We welcome various types of contributions:

### 🐛 Bug Reports

Found a bug? Please create an issue with:

- **Clear title** describing the problem
- **Steps to reproduce** the issue
- **Expected vs actual behavior**
- **Environment details** (OS, .NET version, Python version)
- **Code sample** if applicable

### 💡 Feature Requests

Have an idea for improvement? Create an issue with:

- **Use case** description
- **Proposed solution** or API design
- **Alternative approaches** considered
- **Breaking change** assessment

### 📝 Documentation

Documentation improvements are always welcome:

- Fix typos or unclear explanations
- Add examples and use cases
- Improve API documentation
- Create tutorials or guides

### 🔧 Code Contributions

Ready to code? Great! Please:

- Start with good first issues labeled `good-first-issue`
- Discuss larger changes in an issue first
- Follow our coding standards
- Include tests for new functionality
- Update documentation as needed

## Development Setup

### Building the Project

```bash
# Build the solution
dotnet build

# Run tests
dotnet test

# Build documentation (if you have MkDocs installed)
mkdocs serve
```

### Project Structure

```
CSnakes/
├── src/
│   ├── CSnakes.Runtime/          # Core runtime library
│   ├── CSnakes.SourceGeneration/  # Source generators
│   └── CSnakes.Tests/            # Unit tests
├── samples/                      # Example projects
├── docs/                        # Documentation
└── templates/                   # Project templates
```

### Key Components

- **CSnakes.Runtime**: Core library for Python interop
- **CSnakes.SourceGeneration**: Roslyn source generators
- **Type Converters**: Handle .NET ↔ Python type mapping
- **Environment Management**: Python environment discovery and setup

## Contribution Workflow

### 1. Create a Branch

```bash
# Update your main branch
git checkout main
git pull upstream main

# Create a feature branch
git checkout -b feature/your-feature-name
```

### Branch Naming Convention

- `feature/description` - New features
- `bugfix/description` - Bug fixes
- `docs/description` - Documentation changes
- `refactor/description` - Code refactoring

### 2. Make Changes

- Write clear, focused commits
- Follow coding standards
- Add tests for new functionality
- Update documentation

### 3. Test Your Changes

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test src/CSnakes.Tests/

# Test with different Python versions (if available)
dotnet test --logger "console;verbosity=detailed"
```

### 4. Submit Pull Request

1. **Push your branch**:
   ```bash
   git push origin feature/your-feature-name
   ```

2. **Create Pull Request** on GitHub with:
   - Clear title and description
   - Reference to related issues
   - Summary of changes
   - Breaking changes (if any)
   - Testing performed

3. **Respond to feedback** and update as needed

## Code Standards

### C# Coding Style

We follow Microsoft's C# coding conventions with some specific guidelines:

#### Naming Conventions

```csharp
// Classes and methods: PascalCase
public class PythonEnvironment
{
    public void ExecuteScript() { }
}

// Private fields: _camelCase
private readonly IPythonEnvironment _environment;

// Local variables and parameters: camelCase
public void ProcessData(string inputData)
{
    var resultData = ProcessInput(inputData);
}

// Constants: PascalCase
public const string DefaultPythonVersion = "3.11";
```

#### Code Organization

```csharp
// 1. Using statements (grouped and sorted)
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

// 2. Namespace
namespace CSnakes.Runtime;

// 3. Class with clear documentation
/// <summary>
/// Manages Python environment lifecycle and execution.
/// </summary>
public class PythonEnvironment : IPythonEnvironment
{
    // 4. Private fields
    private readonly ILogger<PythonEnvironment> _logger;
    
    // 5. Constructor
    public PythonEnvironment(ILogger<PythonEnvironment> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    // 6. Public properties
    public string Version { get; }
    
    // 7. Public methods
    public void Execute(string script)
    {
        // Implementation
    }
    
    // 8. Private methods
    private void Cleanup()
    {
        // Implementation
    }
}
```

#### Error Handling

```csharp
// Use specific exception types
throw new ArgumentNullException(nameof(parameter));
throw new InvalidOperationException("Environment not initialized");

// Wrap Python exceptions appropriately
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to execute Python script");
    throw new PythonInvocationException("Script execution failed", ex);
}

// Use using statements for disposables
using var pyObject = environment.Execute("return [1, 2, 3]");
var list = pyObject.As<List<int>>();
```

### Python Code Style

For Python code in tests and samples:

- Follow **PEP 8** style guide
- Use **type hints** where possible
- Include **docstrings** for functions
- Use **meaningful variable names**

```python
def calculate_average(numbers: List[float]) -> float:
    """
    Calculate the average of a list of numbers.
    
    Args:
        numbers: List of numeric values
        
    Returns:
        The arithmetic mean of the input numbers
        
    Raises:
        ValueError: If the list is empty
    """
    if not numbers:
        raise ValueError("Cannot calculate average of empty list")
    
    return sum(numbers) / len(numbers)
```

## Testing Guidelines

### Test Organization

```csharp
[TestClass]
public class PythonEnvironmentTests
{
    private IPythonEnvironment _environment;
    
    [TestInitialize]
    public void Setup()
    {
        _environment = CreateTestEnvironment();
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _environment?.Dispose();
    }
    
    [TestMethod]
    public void Execute_ValidScript_ReturnsExpectedResult()
    {
        // Arrange
        const string script = "2 + 2";
        
        // Act
        var result = _environment.Execute(script);
        
        // Assert
        Assert.AreEqual(4, result.As<int>());
    }
    
    [TestMethod]
    [ExpectedException(typeof(PythonInvocationException))]
    public void Execute_InvalidScript_ThrowsException()
    {
        // Arrange
        const string script = "invalid syntax";
        
        // Act
        _environment.Execute(script);
        
        // Assert handled by ExpectedException
    }
}
```

### Test Categories

Use test categories to organize tests:

```csharp
[TestCategory("Unit")]
[TestCategory("Integration")]
[TestCategory("Performance")]
[TestCategory("Environment")]
```

### Python Test Files

Create Python test files in `test_data` directories:

```
src/CSnakes.Tests/
├── TestData/
│   ├── simple_functions.py
│   ├── complex_types.py
│   └── error_cases.py
└── Unit/
    ├── TypeConversionTests.cs
    └── EnvironmentTests.cs
```

## Documentation

### XML Documentation

All public APIs must include XML documentation:

```csharp
/// <summary>
/// Executes a Python expression and returns the result.
/// </summary>
/// <param name="expression">The Python expression to execute</param>
/// <returns>A PyObject containing the execution result</returns>
/// <exception cref="ArgumentNullException">Thrown when expression is null</exception>
/// <exception cref="PythonInvocationException">Thrown when Python execution fails</exception>
/// <example>
/// <code>
/// var result = environment.ExecuteExpression("2 + 2");
/// var value = result.As&lt;int&gt;();
/// </code>
/// </example>
public PyObject ExecuteExpression(string expression)
```

### Markdown Documentation

- Use clear headings and structure
- Include code examples
- Provide context and use cases
- Link to related documentation

### Example Code

All examples should be:

- **Complete and runnable**
- **Well-commented**
- **Realistic use cases**
- **Error-handling aware**

```csharp
// ✅ Good example
try
{
    using var env = new PythonEnvironmentBuilder()
        .WithPythonVersion("3.11")
        .Build();
    
    var result = env.ExecuteExpression("import sys; sys.version");
    Console.WriteLine($"Python version: {result.As<string>()}");
}
catch (PythonRuntimeException ex)
{
    Console.WriteLine($"Failed to initialize Python: {ex.Message}");
}

// ❌ Avoid incomplete examples
var result = env.Execute("something");  // Where does env come from?
```

## Release Process

### Versioning

We use [Semantic Versioning](https://semver.org/):

- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Release Checklist

1. **Update version numbers** in:
   - `Directory.Build.props`
   - Package metadata

2. **Update CHANGELOG.md** with:
   - New features
   - Bug fixes
   - Breaking changes
   - Migration guide (if needed)

3. **Test thoroughly**:
   - All unit tests pass
   - Integration tests pass
   - Sample projects build and run

4. **Create release PR**:
   - Review all changes
   - Update documentation
   - Get approvals

5. **Tag and release**:
   - Create Git tag
   - Publish NuGet packages
   - Update GitHub release notes

## Community

### Getting Help

- **GitHub Issues**: For bugs and feature requests
- **GitHub Discussions**: For questions and general discussion
- **Documentation**: Start with our comprehensive docs

### Communication Guidelines

- **Be specific**: Provide enough context for others to help
- **Be patient**: Maintainers and contributors are volunteers
- **Be helpful**: Help others when you can
- **Search first**: Check if your question has been asked before

### Recognition

We appreciate all contributions! Contributors are recognized in:

- **CONTRIBUTORS.md** file
- **Release notes** for significant contributions
- **GitHub contributor graphs**

## Questions?

If you have questions about contributing, please:

1. Check the [FAQ](faq.md)
2. Search existing [GitHub Issues](https://github.com/tonybaloney/CSnakes/issues)
3. Create a new issue with the `question` label
4. Join our community discussions

Thank you for contributing to CSnakes! 🐍✨
