using CSnakes.Runtime.Locators;
using CSnakes.Runtime.Python;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Integration.Tests;

/// <summary>
/// Tests specific to Windows Arm64 platform verification
/// </summary>
public sealed class WindowsArm64Tests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public void CanDetectWindowsArm64Architecture()
    {
        // This test will only be meaningful when running on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return; // Skip test on non-Windows platforms
        }

        if (RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return; // Skip test on non-Arm64 platforms
        }

        // If we reach this point, we're on Windows Arm64 and the environment should work
        Assert.Equal(Architecture.Arm64, RuntimeInformation.ProcessArchitecture);
        Assert.True(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
    }

    [Fact]
    public void CanRunBasicPythonCodeOnWindowsArm64()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return; // Skip test on non-Windows Arm64 platforms
        }

        // Test basic Python functionality - get machine architecture
        var testModule = Env.TestWindowsArm64();
        var machineStr = testModule.GetMachineArchitecture();
        Assert.Contains("Arm64", machineStr, StringComparison.OrdinalIgnoreCase);

        // Test that we can run basic computations
        var result = testModule.TestBasicComputation();
        Assert.Equal(4, result);
    }

    [Theory]
    [InlineData("3.11")]
    [InlineData("3.12")]
    [InlineData("3.13")]
    [InlineData("3.14")]
    public void WindowsArm64SupportedPythonVersionsWork(string pythonVersion)
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return; // Skip test on non-Windows Arm64 platforms
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

        // Verify that the redistributable locator can generate URLs for Windows Arm64
        // We'll test this indirectly by verifying the environment works
        var testModule = Env.TestWindowsArm64();
        var result = testModule.GetSuccessMessage();
        Assert.Equal("Windows Arm64 Python is working!", result);
    }

    [Fact]
    public void CanImportNativeExtensionsOnWindowsArm64()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return; // Skip test on non-Windows Arm64 platforms
        }

        // Test that we can import modules that typically have native extensions
        var testModule = Env.TestWindowsArm64();

        // Test importing json (built-in but has C extensions)
        var jsonResult = testModule.TestJsonDumps();
        Assert.Equal("{\"test\": \"value\"}", jsonResult);

        // Test importing _hashlib (native extension)
        var hashlibResult = testModule.TestHashlibImport();
        Assert.True(hashlibResult);

        // Test importing datetime (has C extensions)
        var datetimeResult = testModule.GetDatetimeNowIso();
        Assert.NotNull(datetimeResult);
    }

    [Fact]
    public void CanVerifyPythonLibraryPath()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return; // Skip test on non-Windows Arm64 platforms
        }

        // Test that we can get information about the Python installation
        var testModule = Env.TestWindowsArm64();
        var platform = testModule.GetPlatformInfo();
        Assert.Contains("win", platform, StringComparison.OrdinalIgnoreCase);

        // Test that the executable path exists
        var executable = testModule.GetExecutablePath();
        Assert.NotNull(executable);
        Assert.NotEmpty(executable);
    }

    [Fact]
    public void CanTestNativeCryptographyModules()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test hashlib with various algorithms that may have native implementations
        var testModule = Env.TestWindowsArm64();
        var md5Result = testModule.TestMd5Hash();
        Assert.Equal("098f6bcd4621d373cade4e832627b4f6", md5Result);

        var sha256Result = testModule.TestSha256Hash();
        Assert.Equal("9f86d081884c7d659a2feaa0c55ad015a3bf4f1b2b0b822cd15d6c15b0f00a08", sha256Result);

        // Test _ssl module (critical for HTTPS)
        var sslVersion = testModule.GetOpensslVersion();
        Assert.NotNull(sslVersion);
        Assert.Contains("OpenSSL", sslVersion);

        // Test that we can create SSL contexts (common failure point on Arm64)
        var sslContext = testModule.TestSslContextCreation();
        Assert.True(sslContext);
    }

    [Fact]
    public void CanTestNumericAndMathModules()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test math module with floating point operations that may differ on Arm64
        var testModule = Env.TestWindowsArm64();
        var mathResult = testModule.TestMathSqrt();
        Assert.True(Math.Abs(mathResult - Math.Sqrt(2)) < 1e-10);

        // Test decimal module (critical for financial calculations)
        var decimalResult = testModule.TestDecimalAddition();
        Assert.Equal("0.3", decimalResult);

        // Test struct module for binary data handling (endianness concerns on ARM)
        var structTest = testModule.TestStructPack();
        Assert.Equal(4, structTest.Length);
        // On little-endian systems (including Arm64), should be [0x78, 0x56, 0x34, 0x12]
        Assert.Equal(0x78, structTest[0]);
        Assert.Equal(0x56, structTest[1]);
        Assert.Equal(0x34, structTest[2]);
        Assert.Equal(0x12, structTest[3]);
    }

    [Fact]
    public void CanTestBasicPlatformDetection()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test basic platform detection
        var testModule = Env.TestWindowsArm64();
        var machine = testModule.GetMachineArchitecture();
        Assert.Contains("Arm64", machine, StringComparison.OrdinalIgnoreCase);

        // System should be Windows
        var system = testModule.GetSystemName();
        Assert.Equal("Windows", system);

        // Verify 64-bit architecture
        var arch = testModule.GetArchitectureBits();
        Assert.Equal("64bit", arch);

        // Verify little-endian byte order (Arm64 Windows is little-endian)
        var byteorder = testModule.GetByteOrder();
        Assert.Equal("little", byteorder);

        // Verify sys.maxsize indicates 64-bit
        var maxsize = testModule.GetSysMaxsize();
        Assert.True(maxsize > 2147483647); // Greater than 32-bit max
    }

    [Fact]
    public void CanTestBasicSubprocessOperations()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test subprocess execution (common issue with cross-compiled binaries)
        var testModule = Env.TestWindowsArm64();
        var echoResult = testModule.TestSubprocessEcho();
        Assert.Equal("Arm64_TEST", echoResult);

        // Test os module system calls
        var osName = testModule.GetOsName();
        Assert.Equal("nt", osName); // Windows

        var pathSep = testModule.GetPathSeparator();
        Assert.Equal(";", pathSep); // Windows path separator
    }

    [Fact]
    public void CanTestBasicMemoryOperations()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test ctypes (critical for native interop)
        // Verify 64-bit pointers on Arm64
        var testModule = Env.TestWindowsArm64();
        var ptrSize = testModule.GetPointerSize();
        Assert.Equal(8, ptrSize); // 64-bit pointers

        // Verify array itemsizes match expected Arm64 values
        var intItemsize = testModule.GetIntArrayItemsize();
        Assert.Equal(4, intItemsize); // 32-bit integers

        var floatItemsize = testModule.GetFloatArrayItemsize();
        Assert.Equal(4, floatItemsize); // 32-bit floats
    }

    [Fact]
    public void CanTestBasicUnicodeProcessing()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test Unicode handling (critical for international applications)
        var testString = "Hello 世界 🌍 Ñoño";
        var testModule = Env.TestWindowsArm64();
        var unicodeTest = testModule.TestUnicodeLength(testString);
        Assert.True(unicodeTest > testString.Length); // UTF-8 encoding creates more bytes

        // Test regex module (often has platform-specific optimizations)
        var regexTest = testModule.TestRegexFindall();
        Assert.Equal(3, regexTest);
    }

    [Fact]
    public void CanTestBasicEdgeCases()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test that we can handle large integers (bigint implementation)
        var testModule = Env.TestWindowsArm64();
        var bigIntTest = testModule.TestBigInteger();
        Assert.Equal("16069380442589902755", bigIntTest);

        // Test complex number operations
        var complexMagnitude = testModule.TestComplexMagnitude();
        Assert.Equal(5.0, complexMagnitude);

        // Test itertools (C implementation with potential Arm64 considerations)
        var itertoolsTest = testModule.TestItertoolsCombinations();
        Assert.Equal(6, itertoolsTest);

        // Test collections that have C implementations
        var counterL = testModule.TestCounter();
        Assert.Equal(3, counterL);
    }

    [Fact]
    public void CanTestBasicIOOperations()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test io module operations
        var testModule = Env.TestWindowsArm64();
        var stringIOTest = testModule.TestStringIo();
        Assert.Equal("test string", stringIOTest);

        var bytesIOTest = testModule.TestBytesIo();
        Assert.Equal("test bytes", bytesIOTest);
    }

    [Fact]
    public void CanTestArm64SpecificWorkarounds()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        // Test modules that might not have Arm64 wheels - but should be available as built-ins
        var testModule = Env.TestWindowsArm64();
        var moduleCount = testModule.CountAvailableModules();
        Assert.Equal(6, moduleCount);

        // Test architecture-specific performance characteristics
        var performanceTest = testModule.TestPerformanceComputation();
        Assert.True(performanceTest);

        // Test memory alignment and struct packing (Arm64 specific concerns)
        var alignmentTest = testModule.TestStructAlignment();
        Assert.True(alignmentTest);

        // Test that floating point operations work correctly
        var floatTest = testModule.TestFloatingPointPrecision();
        Assert.True(floatTest);

        // Test subprocess with platform detection
        var platformDetect = testModule.TestPlatformMachineDetection();
        Assert.True(platformDetect);
    }

    [Fact]
    public void CanTestArm64CpuFeatures()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        var testModule = Env.TestWindowsArm64();
        var cpuFeatures = testModule.TestArm64CpuFeatures();

        Assert.NotNull(cpuFeatures);
        Assert.Contains("Arm64", cpuFeatures["machine"].As<string>(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Windows", cpuFeatures["system"].As<string>());
        Assert.NotEmpty(cpuFeatures["processor"].As<string>());
    }

    [Fact]
    public void CanTestArm64MemoryAlignment()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        var testModule = Env.TestWindowsArm64();
        var alignments = testModule.TestMemoryAlignmentArm64();

        // Verify ARM64-specific alignments
        Assert.Equal(1, alignments["char"]); // char is 1 byte
        Assert.Equal(2, alignments["short"]); // short is 2 bytes
        Assert.Equal(4, alignments["int"]); // int is 4 bytes
        Assert.Equal(8, alignments["pointer"]); // pointers are 8 bytes on ARM64
        Assert.Equal(4, alignments["float"]); // float is 4 bytes
        Assert.Equal(8, alignments["double"]); // double is 8 bytes
    }

    [Fact]
    public void CanTestArm64SimdAvailability()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        var testModule = Env.TestWindowsArm64();
        var simdAvailable = testModule.TestArm64SimdAvailability();

        // SIMD operations should be available on ARM64
        Assert.True(simdAvailable);
    }

    [Fact]
    public void CanTestArm64ProcessInformation()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        var testModule = Env.TestWindowsArm64();
        var processInfo = testModule.TestArm64ProcessInformation();

        Assert.True(processInfo["pid"].As<int>() > 0);
        Assert.NotEmpty(processInfo["executable"].As<string>());
        Assert.Equal("little", processInfo["byteorder"].As<string>());
        Assert.Equal("win32", processInfo["platform"].As<string>());
        Assert.Equal("cpython", processInfo["implementation"].As<string>());
        Assert.True(processInfo["maxsize"].As<long>() > 2147483647); // 64-bit
    }

    [Fact]
    public void CanTestArm64ThreadingPerformance()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        var testModule = Env.TestWindowsArm64();
        var threadingResults = testModule.TestArm64ThreadingPerformance();

        Assert.True(threadingResults["thread_creation_time"] > 0);
        Assert.True(threadingResults["lock_contention_time"] > 0);
        Assert.Equal(4000, threadingResults["final_counter"]); // 4 threads * 1000 increments
    }

    [Fact]
    public void CanTestArm64MultiprocessingSupport()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        var testModule = Env.TestWindowsArm64();
        var multiprocessingWorks = testModule.TestArm64MultiprocessingSupport();

        // Multiprocessing should work on Windows ARM64
        Assert.True(multiprocessingWorks);
    }

    [Fact]
    public void CanTestArm64FileSystemPerformance()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        var testModule = Env.TestWindowsArm64();
        var fsResults = testModule.TestArm64FileSystemPerformance();

        Assert.True(fsResults["file_creation_time"] > 0);
        Assert.True(fsResults["file_reading_time"] > 0);
        Assert.True(fsResults["total_bytes_read"] > 0);
    }

    [Fact]
    public void CanTestArm64NetworkStack()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        var testModule = Env.TestWindowsArm64();
        var networkResults = testModule.TestArm64NetworkStack();

        Assert.True(networkResults.ContainsKey("hostname_resolution"));
        Assert.True(networkResults.ContainsKey("supported_families"));
        Assert.True(networkResults["supported_families"].As<int>() >= 1); // At least IPv4
    }

    [Fact]
    public void CanTestArm64GarbageCollection()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        var testModule = Env.TestWindowsArm64();
        var gcResults = testModule.TestArm64GarbageCollection();

        Assert.True(gcResults["gc_time"].As<double>() >= 0);
        Assert.True(gcResults["objects_collected"].As<int>() >= 0);
        Assert.True(gcResults["object_creation_time"].As<double>() > 0);
        Assert.True(gcResults["object_cleanup_time"].As<double>() >= 0);
    }

    [Fact]
    public void CanTestArm64UuidGeneration()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        var testModule = Env.TestWindowsArm64();
        var uuidResults = testModule.TestArm64UuidGeneration();

        Assert.True(uuidResults["generation_time"].As<double>() > 0);
        Assert.Equal(1000, uuidResults["total_generated"].As<int>());
        Assert.True(uuidResults["all_unique"].As<bool>()); // All UUIDs should be unique
        Assert.True(uuidResults["uuid_types_count"].As<int>() >= 2); // At least uuid1 and uuid4
    }

    [Fact]
    public void CanTestArm64ExceptionHandling()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        var testModule = Env.TestWindowsArm64();
        var exceptionResults = testModule.TestArm64ExceptionHandling();

        Assert.True(exceptionResults["basic_exception_handling"]);
        Assert.True(exceptionResults["nested_exception_handling"]);
        Assert.True(exceptionResults["deep_stack_exception"]);
    }

    [Fact]
    public void CanTestArm64WarningSystem()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        var testModule = Env.TestWindowsArm64();
        var warningResults = testModule.TestArm64WarningSystem();

        Assert.Equal(2, warningResults["warnings_captured"].As<int>());
        Assert.True(warningResults["warning_filter_works"].As<bool>());
    }

    [Fact]
    public void CanTestArm64MemoryUsagePatterns()
    {
        // Skip if not on Windows Arm64
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
        {
            return;
        }

        var testModule = Env.TestWindowsArm64();
        var memoryResults = testModule.TestArm64MemoryUsagePatterns();

        // Verify memory sizes are reasonable for ARM64
        Assert.True(memoryResults["int_size"] >= 28); // Python int object overhead
        Assert.True(memoryResults["float_size"] >= 24); // Python float object overhead
        Assert.True(memoryResults["string_size"] > 0);
        Assert.True(memoryResults["list_size"] > 0);
        Assert.True(memoryResults["dict_size"] > 0);
        Assert.True(memoryResults["large_list_size"] > memoryResults["list_size"]);
        Assert.True(memoryResults["large_dict_size"] > memoryResults["dict_size"]);
    }
}
