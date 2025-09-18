using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using CSnakes.Runtime;
using CSnakes.Runtime.Python;
using System.Runtime.InteropServices;

namespace Profile;

/// <summary>
/// Arm64-specific performance benchmarks for Windows Arm64 Python interop.
/// These benchmarks validate performance characteristics and compare Arm64 vs x64 behavior.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
public class Arm64Benchmarks : BaseBenchmark
{
    private IArm64Benchmarks? mod;

    [GlobalSetup]
    public void Setup()
    {
        mod = Env.Arm64Benchmarks();
    }

    /// <summary>
    /// Benchmark Arm64 architecture detection performance
    /// </summary>
    [Benchmark]
    public string DetectArchitecture()
    {
        return mod!.DetectArchitecture();
    }

    /// <summary>
    /// Benchmark platform information retrieval on Arm64
    /// </summary>
    [Benchmark]
    public string GetPlatformInfo()
    {
        return mod!.GetPlatformInfo();
    }

    /// <summary>
    /// Test mathematical operations performance on Arm64
    /// </summary>
    [Benchmark]
    public double MathOperationsArm64()
    {
        return mod!.MathOperationsArm64();
    }

    /// <summary>
    /// Benchmark large data marshalling on Arm64
    /// </summary>
    [Benchmark]
    public int LargeDataMarshalling()
    {
        return (int)mod!.LargeDataMarshalling();
    }

    /// <summary>
    /// Test string processing performance on Arm64
    /// </summary>
    [Benchmark]
    public int StringProcessingArm64()
    {
        return (int)mod!.StringProcessingArm64();
    }

    /// <summary>
    /// Benchmark environment variable access on Arm64
    /// </summary>
    [Benchmark]
    public string EnvironmentAccess()
    {
        return mod!.EnvironmentAccess();
    }

    /// <summary>
    /// Test native module import performance (if available)
    /// </summary>
    [Benchmark]
    public string NativeModulePerformance()
    {
        return mod!.NativeModulePerformance();
    }

    /// <summary>
    /// Benchmark Python function call overhead on Arm64
    /// </summary>
    [Benchmark]
    public double FunctionCallOverhead()
    {
        return mod!.FunctionCallOverhead();
    }

    /// <summary>
    /// Test complex object creation and manipulation on Arm64
    /// </summary>
    [Benchmark]
    public int ComplexObjectManipulation()
    {
        return (int)mod!.ComplexObjectManipulation();
    }

    /// <summary>
    /// Benchmark runtime architecture detection
    /// </summary>
    [Benchmark]
    public string RuntimeArchitectureDetection()
    {
        var dotnetArch = RuntimeInformation.ProcessArchitecture.ToString();
        var osArch = RuntimeInformation.OSArchitecture.ToString();
        var pythonArch = mod!.RuntimeArchitectureDetection();
        
        return $"{dotnetArch}-{pythonArch}-{osArch}";
    }

    /// <summary>
    /// Test exception handling performance on Arm64
    /// </summary>
    [Benchmark]
    public int ExceptionHandlingPerformance()
    {
        return (int)mod!.ExceptionHandlingPerformance();
    }

    /// <summary>
    /// Test Arm64-specific optimizations and native performance
    /// </summary>
    [Benchmark]
    public string Arm64NativePerformance()
    {
        return mod!.Arm64NativePerformance();
    }
}

/// <summary>
/// Arm64-specific configuration for benchmarks
/// </summary>
public class Arm64BenchmarkConfig : ManualConfig
{
    public Arm64BenchmarkConfig()
    {
        AddJob(Job.Default
            .WithRuntime(CoreRuntime.Core80)
            .WithPlatform(Platform.Arm64)
            .WithId("NET8-Arm64"));
            
        AddJob(Job.Default
            .WithRuntime(CoreRuntime.Core90)
            .WithPlatform(Platform.Arm64)
            .WithId("NET9-Arm64"));
    }
}