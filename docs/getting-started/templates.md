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

**Options:**
- `--framework` - Target framework (net8.0, net9.0)
- `--use-top-level-statements` - Use top-level statements (default: true)

### Web Application Template (`pyweb`)

Creates an ASP.NET Core web application with CSnakes integration.

```bash
dotnet new pyweb
```

**What it creates:**
- ASP.NET Core web application
- Python modules for web-specific tasks
- Dependency injection setup
- Example controllers using Python functions

### Library Template (`pylib`)

Creates a class library project configured for CSnakes.

```bash
dotnet new pylib
```

**What it creates:**
- Class library project
- Python modules
- Public API that wraps Python functionality
- Unit test project (optional)

## Template Options

### Common Options

All templates support these options:

- `--name` - Project name
- `--output` - Output directory
- `--framework` - Target framework
- `--langVersion` - C# language version

### Python-Specific Options

- `--python-version` - Python version to target (3.9-3.13)
- `--use-venv` - Set up virtual environment (default: true)
- `--include-requirements` - Include requirements.txt (default: true)

## Examples

### Create a Named Project

```bash
dotnet new pyapp --name MyPythonApp --output ./MyPythonApp
```

### Create with Specific Framework

```bash
dotnet new pyapp --framework net9.0
```

### Create Web App with Virtual Environment

```bash
dotnet new pyweb --use-venv true --include-requirements true
```

## Template Structure

### Console App Template Structure

```
MyApp/
├── MyApp.csproj
├── Program.cs
├── python_modules/
│   ├── __init__.py
│   └── demo.py
└── requirements.txt
```

### Web App Template Structure

```
MyWebApp/
├── MyWebApp.csproj
├── Program.cs
├── Controllers/
│   └── PythonController.cs
├── python_modules/
│   ├── __init__.py
│   ├── data_processing.py
│   └── ml_models.py
├── requirements.txt
└── appsettings.json
```

## Customizing Templates

You can modify the generated templates by:

1. **Adding Python Dependencies**: Edit `requirements.txt`
2. **Adding Python Modules**: Create new `.py` files in the python_modules directory
3. **Configuring Environment**: Modify the Python environment setup in `Program.cs`

## Installing Templates

### Install Latest Version

```bash
dotnet new install CSnakes.Templates
```

### Install Specific Version

```bash
dotnet new install CSnakes.Templates::0.16.0
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
