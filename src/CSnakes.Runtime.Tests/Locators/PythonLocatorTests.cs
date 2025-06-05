using CSnakes.Runtime.Locators;
using System;
using System.Runtime.InteropServices;

namespace CSnakes.Runtime.Tests.Locators;

public class PythonLocatorTests
{
    private static readonly Version PythonVersion = new(3, 9);

    [Fact]
    public void GetPythonExecutablePath_Windows_ReturnsCorrectPath()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip on non-Windows
        }
        const string folder = @"C:\Python39";
        MockPythonLocator locator = new(PythonVersion);

        string result = locator.GetPythonExecutablePathReal(folder);

        Assert.Equal(@"C:\Python39\python.exe", result);
    }

    [Fact]
    public void GetPythonExecutablePath_OSX_ReturnsCorrectPath()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return; // Skip on non-OSX
        }
        const string folder = "/usr/local/python3";
        MockPythonLocator locator = new(PythonVersion);

        string result = locator.GetPythonExecutablePathReal(folder);

        Assert.Equal("/usr/local/python3/bin/python3", result);
    }

    [Fact]
    public void GetPythonExecutablePath_Linux_ReturnsCorrectPath()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return; // Skip on non-Linux
        }
        const string folder = "/usr/local/python3";
        MockPythonLocator locator = new(PythonVersion);

        string result = locator.GetPythonExecutablePathReal(folder);

        Assert.Equal("/usr/local/python3/bin/python3", result);
    }

    [Theory]
    [InlineData(false, @"C:\Python39\python39.dll")]
    [InlineData(true, @"C:\Python39\python39t.dll")]
    public void GetLibPythonPath_Windows_ReturnsCorrectPath(bool freeThreaded, string expected)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip on non-Windows
        }
        const string folder = @"C:\Python39";
        MockPythonLocator locator = new(PythonVersion);

        string result = locator.GetLibPythonPathReal(folder, freeThreaded);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(false, @"/usr/local/python3/lib/libpython3.9.dylib")]
    [InlineData(true, @"/usr/local/python3/lib/libpython3.9t.dylib")]
    public void GetLibPythonPath_OSX_ReturnsCorrectPath(bool freeThreaded, string expected)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return; // Skip on non-OSX
        }
        const string folder = "/usr/local/python3";
        MockPythonLocator locator = new(PythonVersion);

        string result = locator.GetLibPythonPathReal(folder, freeThreaded);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(false, @"/usr/local/python3/lib/libpython3.9.so")]
    [InlineData(true, @"/usr/local/python3/lib/libpython3.9t.so")]
    public void GetLibPythonPath_Linux_ReturnsCorrectPath(bool freeThreaded, string expected)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return; // Skip on non-Linux
        }
        const string folder = "/usr/local/python3";
        MockPythonLocator locator = new(PythonVersion);

        string result = locator.GetLibPythonPathReal(folder, freeThreaded);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetPythonPath_Windows_ReturnsCorrectPath()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip on non-Windows
        }
        const string folder = @"C:\Python39";
        MockPythonLocator locator = new(PythonVersion);

        string result = locator.GetPythonPathReal(folder);

        Assert.Equal(@"C:\Python39\Lib;C:\Python39\DLLs", result);
    }

    [Fact]
    public void GetPythonPath_OSX_ReturnsCorrectPath()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return; // Skip on non-OSX
        }
        const string folder = "/usr/local/python3";
        MockPythonLocator locator = new(PythonVersion);

        string result = locator.GetPythonPathReal(folder);

        Assert.Equal(@"/usr/local/python3/lib/python3.9:/usr/local/python3/lib/python3.9/lib-dynload", result);
    }

    [Fact]
    public void GetPythonPath_Linux_ReturnsCorrectPath()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return; // Skip on non-Linux
        }
        const string folder = "/usr/local/python3";
        MockPythonLocator locator = new(PythonVersion);

        string result = locator.GetPythonPathReal(folder);

        Assert.Equal(@"/usr/local/python3/lib/python3.9:/usr/local/python3/lib/python3.9/lib-dynload", result);
    }

    [Fact]
    public void LocatePythonInternal_throws_if_folder_doesnt_exist()
    {
        const string folder = "folder that doesn't exist";
        MockPythonLocator locator = new(PythonVersion);

        var ex = Assert.Throws<DirectoryNotFoundException>(() => _ = locator.LocatePythonInternalMock(folder));
        Assert.Equal($"Python not found in '{folder}'.", ex.Message);
    }

    [Fact]
    public void LocatePythonInternal_Windows_returns_expected()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip on non-Windows
        }
        // We need a folder that exists, but we don't want to rely on the actual Python installation.
        string folder = Environment.GetEnvironmentVariable("TEMP")!;
        MockPythonLocator locator = new(PythonVersion);

        PythonLocationMetadata result = locator.LocatePythonInternalMock(folder);

        Assert.NotNull(result);
        Assert.Equal(folder, result.Folder);
    }

    [Fact]
    public void LocatePythonInternal_OSX_returns_expected()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return; // Skip on non-OSX
        }
        // We need a folder that exists, but we don't want to rely on the actual Python installation.
        const string folder = "/usr/local";
        MockPythonLocator locator = new(PythonVersion);

        PythonLocationMetadata result = locator.LocatePythonInternalMock(folder);

        Assert.NotNull(result);
        Assert.Equal(folder, result.Folder);
    }

    [Fact]
    public void LocatePythonInternal_Linux_returns_expected()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return; // Skip on non-Linux
        }
        // We need a folder that exists, but we don't want to rely on the actual Python installation.
        const string folder = "/usr/local";
        MockPythonLocator locator = new(PythonVersion);

        PythonLocationMetadata result = locator.LocatePythonInternalMock(folder);

        Assert.NotNull(result);
        Assert.Equal(folder, result.Folder);
    }

    private class MockPythonLocator(Version version) : PythonLocator
    {
        protected override Version Version { get; } = version;

        public string GetPythonExecutablePathReal(string folder) => base.GetPythonExecutablePath(folder);

        public string GetPythonExecutablePathMock(string folder) => GetPythonExecutablePath(folder);

        public string GetLibPythonPathReal(string folder, bool freeThreaded = false) => base.GetLibPythonPath(folder, freeThreaded);

        public string GetLibPythonPathMock(string folder, bool freeThreaded = false) => GetLibPythonPath(folder, freeThreaded);

        public string GetPythonPathReal(string folder) => base.GetPythonPath(folder);

        public string GetPythonPathMock(string folder) => GetPythonPath(folder);

        public PythonLocationMetadata LocatePythonInternalMock(string folder, bool freeThreaded = false) => LocatePythonInternal(folder, freeThreaded);

        public override PythonLocationMetadata LocatePython()
        {
            throw new NotImplementedException();
        }

        protected override string GetLibPythonPath(string folder, bool freeThreaded = false) => "GetLibPythonPath";

        protected override string GetPythonExecutablePath(string folder, bool freeThreaded = false) => "GetPythonExecutablePath";

        protected override string GetPythonPath(string folder, bool freeThreaded = false) => "GetPythonPath";
    }

}
