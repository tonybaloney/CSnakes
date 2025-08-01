# Templates for [CSnakes]

Templates for use with `dotnet new` to create .NET console applications that
use [CSnakes] for interfacing with Python.

## Installation

To install, run:

    dotnet new install CSnakes.Templates

To uninstall, run:

    dotnet new uninstall CSnakes.Templates

## Usage

After installation, use the following command to see the options and usage:

    dotnet new pyapp -h

Once installed, a new console application can be created using the following
command:

    dotnet new pyapp -o ConsoleApp

## Packaging

To package all templates for distribution, run:

    dotnet pack

The package will be created in the `dist` directory.

## Testing

Run the following command to test the templates:

    dotnet tool restore
    dotnet tool run pwsh -NoProfile test.ps1

The tests use verification-based testing. The `tests` contains snapshots of
how a template should be instantiated based on various combinations of
parameters. If tests fail due to a new change in the template, then new
snapshots can be approved by running:

    dotnet tool run pwsh -NoProfile approve.ps1

[CSnakes]: https://tonybaloney.github.io/CSnakes/
