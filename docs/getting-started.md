# Getting Started

To get started with CSnakes, you need to:

* [Install Python](#installing-python)
* [Put your Python files into a C# Project](#configuring-a-c-project-for-csnakes)
* [Use type annotations for the functions you want to call from C#](#using-type-annotations-for-reflection)
* [Install the `CSnakes` and `CSnakes.Runtime` packages into the project](#installing-the-nuget-packages-for-csnakes)
* [Mark them for source generation](#marking-files-for-generation)
* [Install the `CSnakes.Runtime` nuget package in the C# Project you want to execute the Python code from](#building-the-project)
* [Setup a Virtual Environment (Optional)](#using-virtual-environments)
* [Instantiate a Python environment in C# and run the Python function](#calling-csnakes-code-from-cnet)

## Installing Python

## Configuring a C# Project for CSnakes

## Using type annotations for reflection

See the [reference supported types](reference.md#supported-types) for a list of Python types and their C#.NET equivalents.

## Installing the nuget packages for CSnakes

## Marking files for generation

You can either mark a file as an "Additional File" in the CSProj file XML:

```xml
    <ItemGroup>
        <AdditionalFiles Include="hello_world.py">
            <CopyToOutputDirectory>Always</CopyToOutputDirectory>
        </AdditionalFiles>
```

Or, in Visual Studio change the properties of the file and set **Build Action** to **Csharp analyzer additional file**.

## Building the project

## Using Virtual Environments

Since most Python projects require external dependencies outside of the Python standard library, CSnakes supports execution within a Python virtual environment.

## Calling CSnakes code from C#.NET

