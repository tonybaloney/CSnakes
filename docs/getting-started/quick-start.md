# Quick Start

The fastest way to get started with CSnakes is to use the supplied template for a C# console project with the `dotnet new` command.

## Using Templates

The [templates](https://www.nuget.org/packages/CSnakes.Templates) are installed by running the following command:

```bash
dotnet new install CSnakes.Templates
```

To create a C# console application project with the default options, run the following commands:

```bash
mkdir MyApp
cd MyApp
dotnet new pyapp
```

If you run the new application with:

```bash
dotnet run
```

It will print a greetings message from within a Python function that is invoked by C#! This may take a bit the first time you run it because CSnakes will download the Python runtime in a fully isolated environment for the application.

To explore the various options offered by the `pyapp` template, run with the `-h` flag:

```bash
dotnet new pyapp -h
```

## What You Get

The template creates:
- A C# console application with CSnakes.Runtime package
- Example Python files with proper type annotations
- Configured project file with Python files marked for source generation
- A simple example demonstrating Python function calls from C#

## Next Steps

- [Learn about installation](installation.md) for manual setup
- [Try the first example](first-example.md) to understand the basics
- [Explore templates](templates.md) for different project types
