# CSnakes - .NET Python Integration Framework

CSnakes is a .NET Source Generator and Runtime that embeds Python code and libraries into C#/.NET solutions using Python's C-API for high-performance integration without REST/HTTP overhead.

**Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.**

## Working Effectively

### Environment Setup
- Install both .NET 8 and 9 SDKs:
  ```bash
  # Download and run dotnet-install.sh for both versions
  wget https://dot.net/v1/dotnet-install.sh && chmod +x dotnet-install.sh
  ./dotnet-install.sh --channel 8.0 --install-dir /tmp/dotnet
  ./dotnet-install.sh --channel 9.0 --install-dir /tmp/dotnet
  export PATH="/tmp/dotnet:$PATH"
  ```
- Verify installation: `dotnet --list-sdks` should show both 8.x and 9.x
- Verify runtimes: `dotnet --list-runtimes` should show both frameworks
- Python 3.9-3.13 is required: `python3 --version`

### Build Process
- **NEVER CANCEL builds or long-running commands**. Set timeouts appropriately.
- Navigate to `src/` directory for all build operations
- **Restore** (10-60 seconds): `dotnet restore` - NEVER CANCEL, set timeout to 120+ seconds
- **Build** (3-25 seconds): `dotnet build --no-restore` - NEVER CANCEL, set timeout to 60+ seconds  
- **Target specific framework**: `dotnet build --no-restore --framework net8.0` (faster, 2-5 seconds)

### Test Execution  
- **CRITICAL LIMITATION**: Runtime tests require native `csnakes_python` library not automatically built
- Unit tests will fail with `DllNotFoundException` for `csnakes_python` library - this is expected
- **If tests must be run**: Set environment variable `export Python3_ROOT_DIR="/usr"` first
- **Test timing**: 60+ seconds when properly configured - NEVER CANCEL, set timeout to 180+ seconds
- **Working alternative**: Build and compile validation only - tests are not essential for development

## Validation

### Build Validation
- Always run full restore and build cycle when making changes
- Verify core projects build: `dotnet build --no-restore CSnakes.Runtime/CSnakes.Runtime.csproj`
- Sample projects build validation: `cd samples/simple && dotnet build --no-restore`
- **Expected build warnings**: Some nullability warnings in source generation code are normal

### Sample Projects
- Sample projects in `samples/simple/` build successfully but **cannot run** due to native library limitation
- Use sample builds to validate source generation and C# compilation works correctly  
- WebApp sample includes API endpoints but requires native library for runtime execution
- **Do not attempt to run samples** - they will fail with missing `csnakes_python` library

### Documentation
- Documentation uses MkDocs with Material theme
- **Skip documentation build** - requires additional Python packages and plugins not essential for development
- Reference online docs at https://tonybaloney.github.io/CSnakes/ instead

## Common Tasks

### Repository Structure
```
/home/runner/work/CSnakes/CSnakes/
├── src/                          # Main .NET solution
│   ├── CSnakes.Runtime/          # Core runtime library  
│   ├── CSnakes.SourceGeneration/ # Roslyn source generator
│   ├── CSnakes.Tests/            # Unit tests
│   ├── Integration.Tests/        # Integration tests
│   └── CSnakes.sln               # Main solution file
├── samples/                      # Example projects
│   └── simple/                   # Basic samples (build-only)
├── docs/                         # MkDocs documentation
└── templates/                    # dotnet new templates
```

### Build Commands Reference
```bash
# Working directory for all builds
cd /home/runner/work/CSnakes/CSnakes/src

# Full clean build (90-120 seconds total)
dotnet restore                     # 10-60s - NEVER CANCEL
dotnet build --no-restore         # 3-25s - NEVER CANCEL  

# Faster build targeting .NET 8 only (15-30 seconds total)  
dotnet restore                     # 10-60s - NEVER CANCEL
dotnet build --no-restore --framework net8.0  # 2-5s

# Sample validation
cd ../samples/simple
dotnet restore                     # 10s - NEVER CANCEL  
dotnet build --no-restore         # 3-5s - NEVER CANCEL
```

### Key Project Files
- `src/CSnakes.sln` - Main solution with 10+ projects
- `src/Directory.Build.props` - Shared build configuration, targets .NET 8/9
- `src/Directory.Packages.props` - Central package management
- `samples/simple/SimpleSamples.sln` - Sample projects solution

## Critical Limitations

### Native Library Dependency
- **Runtime execution requires `csnakes_python` shared library** that provides .NET-to-Python C-API bridge
- This library is **NOT automatically built** during `dotnet build`
- **Samples and tests will fail at runtime** with `DllNotFoundException` - this is expected
- **Development workflow**: Focus on build validation, source generation, and C# compilation
- **Production deployment**: Uses different mechanism to provide native library (not covered in dev setup)

### Environment Requirements
- Both .NET 8 and 9 SDKs required for full compatibility
- Python 3.9-3.13 required but runtime integration limited by native library
- Tests require specific environment variables like `Python3_ROOT_DIR`
- Cross-platform: Windows, macOS, Linux supported in production but dev limitations apply

### Known Working Operations
- ✅ Source code compilation and build
- ✅ Source generator execution  
- ✅ NuGet package creation
- ✅ Sample project compilation
- ❌ Runtime Python integration (native library required)
- ❌ Test execution (native library required)
- ❌ Sample application execution (native library required)

## Development Workflow
1. **Always build first**: `cd src && dotnet restore && dotnet build --no-restore`
2. **Validate changes**: Focus on compilation success, not runtime execution
3. **Test source generation**: Changes to .py files should trigger C# regeneration
4. **Sample validation**: `cd samples/simple && dotnet build --no-restore`
5. **Never try to run applications** - they require native library not available in dev environment

Remember: CSnakes is primarily a **build-time source generation tool** with runtime components that require additional deployment infrastructure.