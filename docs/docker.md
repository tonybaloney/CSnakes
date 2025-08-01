# Running in Docker containers

CSnakes has a command-line tool for installing Python, creating virtual environments, and installing dependencies.

This tool is designed for pre-creating Python environments in Docker images for use in CSnakes projects, but can be used as a general-purpose tool for installing Python. 

## Overview

CSnakes.Stage is designed to streamline Python environment setup for CSnakes applications. It automatically downloads the appropriate Python redistributable for your platform, optionally creates virtual environments, and can install Python packages from requirements files.

## Installation

Install CSnakes.Stage as a .NET global tool:

```bash
dotnet tool install -g CSnakes.Stage
```

## Usage

### Basic Syntax

```bash
setup-python --python <version> [options]
```

### Required Parameters

- `--python <version>` - The Python version to download and use (e.g., `3.12`, `3.11`, `3.10`)

### Optional Parameters

- `--timeout <seconds>` - Timeout for downloading Python redistributable (default: 500 seconds)
- `--venv <path>` - Path where a virtual environment should be created
- `--pip-requirements <path>` - Path to a pip requirements.txt file to install packages
- `--verbose` - Enable detailed output during execution

## Examples

### Download Python 3.12

```bash
setup-python --python 3.12
```

The path to Python will be printed to the console. This is the same path as used in `.FromRedistributable()` in CSnakes applications. (`%APPDATA%\CSnakes\pythonXX`)


### Download Python and Create Virtual Environment

```bash
setup-python --python 3.12 --venv /app/my-venv
```

### Download Python, Create Virtual Environment, and Install Dependencies

```bash
setup-python --python 3.12 --venv /app/my-venv --pip-requirements /src/requirements.txt
```

## Example Dockerfile

This dockerfile shows how to use `setup-python` to set up a Python environment in a Docker image along with a published .NET application:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ExampleApp/ExampleApp.csproj", "ExampleApp/"]
RUN dotnet restore "ExampleApp/ExampleApp.csproj"
COPY . .
WORKDIR "/src/ExampleApp"
RUN dotnet build "./ExampleApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ExampleApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
RUN dotnet tool install --global CSnakes.Stage
ENV PATH="/root/.dotnet/tools:${PATH}"
RUN setup-python --python 3.12 --venv /app/venv --pip-requirements /src/ExampleApp/requirements.txt --verbose

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /root/.config/CSnakes /home/app/.config/CSnakes
COPY --from=publish /app/venv /app/venv
ENTRYPOINT ["dotnet", "ExampleApp.dll"]
```
