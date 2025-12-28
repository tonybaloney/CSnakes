# Configuration

## Discovering Python files for Source Generation

There are two options for configuring how CSnakes discovers Python files in your .NET project to build the C# bindings:

1. [Manually, using `AdditionalFiles`](#manual-example)
2. [Automatic using `DefaultPythonItems`](#automatic-example)

### Manual Example

If you want full control over which files in your project are source generated, use the `AdditionalFiles` group with `SourceItemType="Python"`.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="CSnakes.Runtime" Version="2.*-*" />
  </ItemGroup>

  <ItemGroup>
    <AdditionalFiles Include="math_utils.py" SourceItemType="Python">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </AdditionalFiles>
  </ItemGroup>
</Project>
```

The file path supports expressions (glob), like `*.py` or `**/*.py` (recursive).

```xml
    <AdditionalFiles Include="python/*.py" SourceItemType="Python">
      <CopyToOutputDirectory>Always</CopyToOutputDirectory>
    </AdditionalFiles>
```

### Automatic Example

CSnakes can automatically find any `.py` and `.pyi` files in your project and generated .NET classes using the `DefaultPythonItems` setting:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <!-- use automatic detection -->
    <DefaultPythonItems>true</DefaultPythonItems>
  </PropertyGroup>
</Project>
```

This works recursively, if you have nested modules in your project you may also need to configure the root directory for namespacing Python imports.

## Embedding Sources (Optional)

Sometimes, you might wish to bundle the Python files into the generated .NET assemblies. This makes it easier to distribute packages, as Python doesn't need to read
from a particular folder. Set the `EmbedPythonSources` property to `true` (default `false`) to enable this option.

For this option, the source of the Python file is embedded into the generated class by CSnakes and the `.WithHome` extension method for the Python environment doesn't need
to contain the Python file.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <!-- use embedded sources -->
    <EmbedPythonSources>true</EmbedPythonSources>
  </PropertyGroup>
</Project>
```

This will only operate on Python source files (`.py`) and not typestubs (`.pyi`). The source embedded in the class is automatically updated when you update the Python source and save the file (Visual Studio), or on build for CLI.

## Namespaces and Roots (Optional)

Consider the following folder layout:

```
- `project.csproj`
- `src\`
  - `MyClass.cs`
- `python_src\`
  - `library.py`
  - `utils\`
    - `__init__.py`
    - `parser.py`
```

In Python, this folder structure would be considered a namespaced project with modules and submodules.

If the home were `python_src\`:

- To import `library.py` you would `import library`
- To import `utils\__init__.py` you would `import utils`
- To import `utils\parser.py` you would `import utils.parser`

CSnakes will automatically generate these namespaces and import directives once you configure the root of the project using the `PythonRoot` property in your `.csproj`.

```xml
  <PropertyGroup>
    <DefaultPythonItems>true</DefaultPythonItems>
    <PythonRoot>python_src</PythonRoot>
  </PropertyGroup>
```

This directive sets the root namespace for Python to be the relative path of `python_src`.

You can also provide sub-directories in the path:

```xml
    <PythonRoot>src/python/bindings</PythonRoot>
```

To be platform agnostic, the path should use Unix path formatting (`/` not `\`).

If CSnakes detects any Python sources which are not in this namespace, they will be ignored and a message is outputted to the build logs.
