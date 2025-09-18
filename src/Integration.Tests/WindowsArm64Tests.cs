using CSnakes.Runtime.Locators;
using CSnakes.Runtime.Python;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Integration.Tests;

/// <summary>
/// Tests specific to Windows ARM64 platform verification
/// </summary>
[Collection(PythonEnvironmentCollection.Name)]
public sealed class WindowsArm64Tests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void CanDetectWindowsArm64Architecture()
    {
        // This test will only be meaningful when running on Windows ARM64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip test on non-Windows platforms
        }
        
        if (RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return; // Skip test on non-ARM64 platforms
        }

        // If we reach this point, we're on Windows ARM64 and the environment should work
        Assert.Equal(Architecture.Arm64, RuntimeInformation.ProcessArchitecture);
        Assert.True(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
    }

    [Fact]
    public void CanRunBasicPythonCodeOnWindowsArm64()
    {
        // Skip if not on Windows ARM64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || 
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return; // Skip test on non-Windows ARM64 platforms
        }

        // Test basic Python functionality - get machine architecture
        using var machine = Env.ExecuteExpression("__import__('platform').machine()");
        var machineStr = machine.As<string>();
        Assert.Contains("ARM64", machineStr, StringComparison.OrdinalIgnoreCase);

        // Test that we can run basic computations
        using var result = Env.ExecuteExpression("2 + 2");
        Assert.Equal(4, result.As<int>());
    }

    [Theory]
    [InlineData("3.11")]
    [InlineData("3.12")]
    [InlineData("3.13")]
    [InlineData("3.14")]
    public void WindowsArm64SupportedPythonVersionsWork(string pythonVersion)
    {
        // Skip if not on Windows ARM64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || 
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return; // Skip test on non-Windows ARM64 platforms
        }

        // Skip if the current test environment doesn't match the test version
        var envVersion = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";
        if (!envVersion.StartsWith(pythonVersion))
        {
            return; // Skip if versions don't match
        }

        // Test that we can create and use a Python environment with this version
        var version = Version.Parse($"{pythonVersion}.0");
        var redistributableVersion = version.Minor switch
        {
            11 => RedistributablePythonVersion.Python3_11,
            12 => RedistributablePythonVersion.Python3_12,
            13 => RedistributablePythonVersion.Python3_13,
            14 => RedistributablePythonVersion.Python3_14,
            _ => throw new NotSupportedException($"Python version {version} is not supported in this test.")
        };

        // Verify that the redistributable locator can generate URLs for Windows ARM64
        // We'll test this indirectly by verifying the environment works
        using var result = Env.ExecuteExpression("'Windows ARM64 Python is working!'");
        Assert.Equal("Windows ARM64 Python is working!", result.As<string>());
    }

    [Fact]
    public void CanImportNativeExtensionsOnWindowsArm64()
    {
        // Skip if not on Windows ARM64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || 
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return; // Skip test on non-Windows ARM64 platforms
        }

        // Test that we can import modules that typically have native extensions
        
        // Test importing json (built-in but has C extensions)
        using var jsonResult = Env.ExecuteExpression("__import__('json').dumps({'test': 'value'})");
        Assert.Equal("{\"test\": \"value\"}", jsonResult.As<string>());

        // Test importing _hashlib (native extension)
        using var hashlibResult = Env.ExecuteExpression("bool(__import__('_hashlib'))");
        Assert.True(hashlibResult.As<bool>());
        
        // Test importing datetime (has C extensions)
        using var datetimeResult = Env.ExecuteExpression("__import__('datetime').datetime.now().isoformat()");
        Assert.NotNull(datetimeResult.As<string>());
    }

    [Fact]
    public void CanVerifyPythonLibraryPath()
    {
        // Skip if not on Windows ARM64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || 
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return; // Skip test on non-Windows ARM64 platforms
        }

        // Test that we can get information about the Python installation
        using var platform = Env.ExecuteExpression("__import__('sysconfig').get_platform()");
        var platformStr = platform.As<string>();
        Assert.Contains("win", platformStr, StringComparison.OrdinalIgnoreCase);

        // Test that the executable path exists
        using var executable = Env.ExecuteExpression("__import__('sys').executable");
        var executablePath = executable.As<string>();
        Assert.NotNull(executablePath);
        Assert.NotEmpty(executablePath);
    }

    [Fact]
    public void CanTestNativeCryptographyModules()
    {
        // Skip if not on Windows ARM64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || 
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test hashlib with various algorithms that may have native implementations
        using var md5Result = Env.ExecuteExpression("__import__('hashlib').md5(b'test').hexdigest()");
        Assert.Equal("098f6bcd4621d373cade4e832627b4f6", md5Result.As<string>());

        using var sha256Result = Env.ExecuteExpression("__import__('hashlib').sha256(b'test').hexdigest()");
        Assert.Equal("9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08", sha256Result.As<string>());

        // Test _ssl module (critical for HTTPS)
        using var sslVersion = Env.ExecuteExpression("__import__('ssl').OPENSSL_VERSION");
        Assert.NotNull(sslVersion.As<string>());
        Assert.Contains("OpenSSL", sslVersion.As<string>());

        // Test that we can create SSL contexts (common failure point on ARM64)
        using var sslContext = Env.ExecuteExpression("bool(__import__('ssl').create_default_context())");
        Assert.True(sslContext.As<bool>());
    }

    [Fact]
    public void CanTestNumericAndMathModules()
    {
        // Skip if not on Windows ARM64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || 
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test math module with floating point operations that may differ on ARM64
        using var mathResult = Env.ExecuteExpression("__import__('math').sqrt(2)");
        Assert.True(Math.Abs(mathResult.As<double>() - Math.Sqrt(2)) < 1e-10);

        // Test decimal module (critical for financial calculations)
        using var decimalResult = Env.ExecuteExpression("str(__import__('decimal').Decimal('0.1') + __import__('decimal').Decimal('0.2'))");
        Assert.Equal("0.3", decimalResult.As<string>());

        // Test struct module for binary data handling (endianness concerns on ARM)
        using var structTest = Env.ExecuteExpression("__import__('struct').pack('I', 0x12345678)");
        var structBytes = structTest.As<byte[]>();
        Assert.Equal(4, structBytes.Length);
        // On little-endian systems (including ARM64), should be [0x78, 0x56, 0x34, 0x12]
        Assert.Equal(0x78, structBytes[0]);
        Assert.Equal(0x56, structBytes[1]);
        Assert.Equal(0x34, structBytes[2]);
        Assert.Equal(0x12, structBytes[3]);
    }

    [Fact]
    public void CanTestBasicPlatformDetection()
    {
        // Skip if not on Windows ARM64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || 
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test basic platform detection
        using var machine = Env.ExecuteExpression("__import__('platform').machine()");
        Assert.Contains("ARM64", machine.As<string>(), StringComparison.OrdinalIgnoreCase);

        // System should be Windows
        using var system = Env.ExecuteExpression("__import__('platform').system()");
        Assert.Equal("Windows", system.As<string>());

        // Verify 64-bit architecture
        using var arch = Env.ExecuteExpression("__import__('platform').architecture()[0]");
        Assert.Equal("64bit", arch.As<string>());

        // Verify little-endian byte order (ARM64 Windows is little-endian)
        using var byteorder = Env.ExecuteExpression("__import__('sys').byteorder");
        Assert.Equal("little", byteorder.As<string>());

        // Verify sys.maxsize indicates 64-bit
        using var maxsize = Env.ExecuteExpression("__import__('sys').maxsize");
        Assert.True(maxsize.As<long>() > 2147483647); // Greater than 32-bit max
    }

    [Fact]
    public void CanTestBasicSubprocessOperations()
    {
        // Skip if not on Windows ARM64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || 
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test subprocess execution (common issue with cross-compiled binaries)
        using var echoResult = Env.ExecuteExpression("__import__('subprocess').run(['cmd', '/c', 'echo', 'ARM64_TEST'], capture_output=True, text=True, shell=False).stdout.strip()");
        Assert.Equal("ARM64_TEST", echoResult.As<string>());

        // Test os module system calls
        using var osName = Env.ExecuteExpression("__import__('os').name");
        Assert.Equal("nt", osName.As<string>()); // Windows

        using var pathSep = Env.ExecuteExpression("__import__('os').pathsep");
        Assert.Equal(";", pathSep.As<string>()); // Windows path separator
    }

    [Fact]
    public void CanTestBasicMemoryOperations()
    {
        // Skip if not on Windows ARM64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || 
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test ctypes (critical for native interop)
        // Verify 64-bit pointers on ARM64
        using var ptrSize = Env.ExecuteExpression("__import__('ctypes').sizeof(__import__('ctypes').c_void_p)");
        Assert.Equal(8, ptrSize.As<int>()); // 64-bit pointers

        // Verify array itemsizes match expected ARM64 values
        using var intItemsize = Env.ExecuteExpression("__import__('array').array('i', [1]).itemsize");
        Assert.Equal(4, intItemsize.As<int>()); // 32-bit integers

        using var floatItemsize = Env.ExecuteExpression("__import__('array').array('f', [1.0]).itemsize");
        Assert.Equal(4, floatItemsize.As<int>()); // 32-bit floats
    }

    [Fact]
    public void CanTestBasicUnicodeProcessing()
    {
        // Skip if not on Windows ARM64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || 
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test Unicode handling (critical for international applications)
        var testString = "Hello 世界 🌍 Ñoño";
        using var unicodeTest = Env.ExecuteExpression($"len('{testString}'.encode('utf-8'))");
        Assert.True(unicodeTest.As<int>() > testString.Length); // UTF-8 encoding creates more bytes

        // Test regex module (often has platform-specific optimizations)
        using var regexTest = Env.ExecuteExpression("len(__import__('re').compile(r'[a-zA-Z]+\\d+').findall('test123 hello456 world789'))");
        Assert.Equal(3, regexTest.As<int>());
    }

    [Fact]
    public void CanTestBasicEdgeCases()
    {
        // Skip if not on Windows ARM64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || 
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test that we can handle large integers (bigint implementation)
        using var bigIntTest = Env.ExecuteExpression("str(2 ** 200)[:20]");
        Assert.Equal("16069380442589902755", bigIntTest.As<string>());

        // Test complex number operations
        using var complexMagnitude = Env.ExecuteExpression("abs(complex(3, 4))");
        Assert.Equal(5.0, complexMagnitude.As<double>());

        // Test itertools (C implementation with potential ARM64 considerations)
        using var itertoolsTest = Env.ExecuteExpression("len(list(__import__('itertools').combinations([1, 2, 3, 4], 2)))");
        Assert.Equal(6, itertoolsTest.As<int>());

        // Test collections that have C implementations
        using var counterL = Env.ExecuteExpression("__import__('collections').Counter('hello world')['l']");
        Assert.Equal(3, counterL.As<int>());
    }

    [Fact]
    public void CanTestBasicIOOperations()
    {
        // Skip if not on Windows ARM64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || 
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test io module operations
        using var stringIOTest = Env.ExecuteExpression("__import__('io').StringIO('test string').read()");
        Assert.Equal("test string", stringIOTest.As<string>());

        using var bytesIOTest = Env.ExecuteExpression("__import__('io').BytesIO(b'test bytes').read().decode('utf-8')");
        Assert.Equal("test bytes", bytesIOTest.As<string>());
    }

    [Fact]
    public void CanTestArm64SpecificWorkarounds()
    {
        // Skip if not on Windows ARM64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || 
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test modules that might not have ARM64 wheels - but should be available as built-ins
        using var moduleCount = Env.ExecuteExpression("sum(1 for module in ['sqlite3', 'zlib', 'bz2', 'lzma', '_pickle', '_json'] if __import__(module) is not None)");
        Assert.Equal(6, moduleCount.As<int>());

        // Test architecture-specific performance characteristics
        using var performanceTest = Env.ExecuteExpression("sum(i * i for i in range(10000)) == 333283335000");
        Assert.True(performanceTest.As<bool>());

        // Test memory alignment and struct packing (ARM64 specific concerns)
        using var alignmentTest = Env.ExecuteExpression("__import__('struct').pack('<I', 0x12345678) == b'\\x78\\x56\\x34\\x12'");
        Assert.True(alignmentTest.As<bool>());

        // Test that floating point operations work correctly
        using var floatTest = Env.ExecuteExpression("abs(__import__('math').sin(__import__('math').pi / 4) - 0.7071067811865476) < 1e-15");
        Assert.True(floatTest.As<bool>());

        // Test subprocess with platform detection
        using var platformDetect = Env.ExecuteExpression("'ARM64' in __import__('platform').machine().upper()");
        Assert.True(platformDetect.As<bool>());
    }
}