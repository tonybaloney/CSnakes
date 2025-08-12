# Project Templates

CSnakes provides several project templates to help you get started quickly with different types of applications.

## Available Templates

### Console Application Template (`pyapp`)

Creates a simple console application with CSnakes integration.

```bash
dotnet new pyapp
```

**What it creates:**

- Basic console application
- Example Python file with type annotations
- Configured `.csproj` file
- Simple C# code demonstrating Python function calls

## Template Options

### Common Options

All templates support these standard dotnet new options:

- `--name` (`-n`) - The name for the output being created
- `--output` (`-o`) - Location to place the generated output
- `--dry-run` - Displays a summary of what would happen without creating files
- `--force` - Forces content to be generated even if it would change existing files
- `--no-update-check` - Disables checking for template package updates
- `--project` - The project that should be used for context evaluation
- `--language` (`-lang`) - Specifies the template language (C#)
- `--type` - Specifies the template type (project)

### CSnakes Template-Specific Options

- `--PythonVersion` (`-P`) - Python version to target (default: 3.12)
  - Available options: `3.9`, `3.10`, `3.11`, `3.12`, `3.13`
- `--NoVirtualEnvironment` (`-N`) - Disable virtual environment setup (default: false)
  - When false (default), a virtual environment will be created
- `--PackageManager` (`-Pa`) - Package manager to use (default: none)
  - Available options: `none`, `pip`, `uv`

## Examples

### Create a Named Project

```bash
dotnet new pyapp --name MyPythonApp --output ./MyPythonApp
```

### Create with Specific Python Version

```bash
dotnet new pyapp --PythonVersion 3.11
```

### Create with Package Manager

```bash
dotnet new pyapp --PackageManager pip
```

### Create Without Virtual Environment

```bash
dotnet new pyapp --NoVirtualEnvironment
```

### Create with Multiple Options

```bash
dotnet new pyapp --name DataProcessor --PythonVersion 3.12 --PackageManager uv --output ./data-app
```

## Template Structure

### Console App Template Structure

```
MyApp/
├── MyApp.csproj
├── Program.cs
└── hello.py
```

## Customizing Templates

You can modify the generated templates by:

1. **Adding Python Dependencies**: Add/Edit `requirements.txt`
2. **Adding Python Modules**: Create new `.py` files in the project directory
3. **Configuring Environment**: Modify the Python environment setup in `Program.cs`

## Installing Templates

### Install Latest Version

```bash
dotnet new install CSnakes.Templates
```

### Install Specific Version

```bash
dotnet new install CSnakes.Templates::1.0.1
```

### Update Templates

```bash
dotnet new update
```

### Uninstall Templates

```bash
dotnet new uninstall CSnakes.Templates
```

## Next Steps

- [Understand basic usage](../user-guide/basic-usage.md)
- [Learn about environments](../user-guide/environments.md)
- [Explore sample projects](../examples/sample-projects.md)
