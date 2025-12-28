# Contributing to CSnakes

We welcome contributions to CSnakes! This guide outlines how to contribute to the project, from reporting bugs to submitting code changes.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Types of Contributions](#types-of-contributions)
- [Development Setup](#development-setup)
- [Contribution Workflow](#contribution-workflow)
- [Testing Guidelines](#testing-guidelines)
- [Documentation](#documentation)

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

- **.NET 8.0 and 9.0 SDK**
- **Python 3.10+** installed
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

## Testing Guidelines

- Feature changes should have an integration test in the `Integration.Tests` project
- Bug fixes should first have a unit/integration test demonstrating the bug then a fix in a different commit.

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

- **Release notes** for significant contributions
- **GitHub contributor graphs**

## Questions?

If you have questions about contributing, please:

1. Check the [FAQ](faq.md)
2. Search existing [GitHub Issues](https://github.com/tonybaloney/CSnakes/issues)
3. Create a new issue with the `question` label
4. Join our community [Discussions](https://github.com/tonybaloney/CSnakes/discussions)

Thank you for contributing to CSnakes! 🐍✨
