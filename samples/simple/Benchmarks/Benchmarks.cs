using BenchmarkDotNet.Attributes;
using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.IO;

namespace Benchmarks;

[MemoryDiagnoser]
public class InteropBenchmarks
{
    private IHost _host;

    private IBenchmarks Services { get; set; } // Renamed property to avoid conflict with class name

    [GlobalSetup]
    public void Setup()
    {
        var home = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "python");
        var builder = Host.CreateApplicationBuilder();
        builder.Services
            .WithPython()
            .WithHome(home)
            .FromRedistributable("3.13");
        _host = builder.Build();
        Services = _host.Services.GetRequiredService<IPythonEnvironment>().Benchmarks();
    }

    /// <summary>
    /// Benchmark for Python uuid to .NET Guid letting .NET convert to big-endian
    /// </summary>
    [Benchmark]
    public Guid GuidAsBigEndianUuidTest() =>
        new(Services.NetGuidBigEndian(), true);

    /// <summary>
    /// Benchmark for Python uuid to .NET Guid letting Python convert from little-endian
    /// </summary>
    [Benchmark]
    public Guid GuidAsLittleEndianUuidTest() =>
        new(Services.NetGuidLittleEndian());

    /// <summary>
    /// Benchmark for Python uuid to .NET Guid via string
    /// </summary>
    [Benchmark]
    public Guid GuidAsStringUuidTest() =>
        new(Services.NetGuidStr());

    /// <summary>
    /// Benchmark for .NET Guid to Python uuid via string
    /// </summary>
    [Benchmark]
    public void UuidAsStringGuidTest() =>
        Services.PythonUuidStr(Guid.NewGuid().ToString());

    /// <summary>
    /// Benchmark for .NET Guid to Python uuid letting .NET convert to big-endian
    /// </summary>
    [Benchmark]
    public void UuidAsBigEndianGuidTest() =>
        Services.PythonUuidBigEndian(Guid.NewGuid().ToByteArray(true));

    /// <summary>
    /// Benchmark for .NET Guid to Python uuid letting Python convert from little-endian
    /// </summary>
    [Benchmark]
    public void UuidAsLittleEndianGuidTest() =>
        Services.PythonUuidLittleEndian(Guid.NewGuid().ToByteArray());

    [Benchmark]
    public void DateOnlyAsDateStringTest() =>
        Services.PythonDateStr($"{DateOnly.FromDateTime(DateTime.Now):O}");

    [Benchmark]
    public void DateOnlyAsDateOrdinalTest() =>
        Services.PythonDateOrdinal(DateOnly.FromDateTime(DateTime.Now).DayNumber);

    [Benchmark]
    public DateOnly DateAsDateOnlyStringTest() =>
        DateOnly.Parse(Services.NetDateStr(), CultureInfo.InvariantCulture);

    [Benchmark]
    public DateOnly DateAsDateOnlyOrdinalTest() =>
        DateOnly.FromDayNumber((int)Services.NetDateOrdinal());

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _host.Dispose();
        GC.SuppressFinalize(this);
    }
}
