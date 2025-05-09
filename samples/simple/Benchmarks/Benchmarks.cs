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

    /// <summary>
    /// Benchmark for .NET Guid to Python uuid via string
    /// </summary>
    [Benchmark]
    public void UuidAsStringGuidTest() =>
        Services.PythonUuidStr(Guid.NewGuid().ToString());

    /// <summary>
    /// Benchmark for .NET DateOnly to Python date via string
    /// </summary>
    [Benchmark]
    public void DateOnlyAsDateStringTest() =>
        Services.PythonDateStr($"{DateOnly.FromDateTime(DateTime.Now):O}");

    /// <summary>
    /// Benchmark for .NET DateOnly to Python date letting Python convert to DayNumber
    /// </summary>
    [Benchmark]
    public void DateOnlyAsDateOrdinalTest() =>
        Services.PythonDateOrdinal(DateOnly.FromDateTime(DateTime.Now).DayNumber);

    /// <summary>
    /// Benchmark for Python date to .NET DateOnly via string
    /// </summary>
    [Benchmark]
    public DateOnly DateAsDateOnlyStringTest() =>
        DateOnly.Parse(Services.NetDateStr(), CultureInfo.InvariantCulture);

    /// <summary>
    /// Benchmark for Python date to .NET DateOnly letting Python convert to DayNumber
    /// </summary>
    [Benchmark]
    public DateOnly DateAsDateOnlyOrdinalTest() =>
        DateOnly.FromDayNumber((int)Services.NetDateOrdinal());

    /// <summary>
    /// Benchmark for Python time to .NET TimeOnly via string
    /// </summary>
    [Benchmark]
    public TimeOnly TimeAsTimeOnlyStringTest() =>
        TimeOnly.Parse(Services.NetTimeStr(), CultureInfo.InvariantCulture);

    /// <summary>
    /// Benchmark for Python time to .NET TimeOnly via microseconds
    /// </summary>
    [Benchmark]
    public TimeOnly TimeAsTimeOnlyMicrosecondsTest() =>
        TimeOnly.FromTimeSpan(TimeSpan.FromMicroseconds(Services.NetTimeMicroseconds()));

    /// <summary>
    /// Benchmark for .NET TimeOnly to Python time via string
    /// </summary>
    [Benchmark]
    public void TimeOnlyAsTimeStringTest() =>
        Services.PythonTimeStr($"{TimeOnly.FromDateTime(DateTime.Now):O}");

    /// <summary>
    /// Benchmark for .NET TimeOnly to Python time via microseconds
    /// </summary>
    [Benchmark]
    public void TimeOnlyAsTimeMicrosecondsTest() =>
        Services.PythonTimeMicroseconds(TimeOnly.FromDateTime(DateTime.Now).Ticks / TimeSpan.TicksPerMicrosecond);

    /// <summary>
    /// Benchmark for Python timedelta to .NET TimeSpan via string
    /// </summary>
    [Benchmark]
    public TimeSpan TimeDeltaAsTimeSpanStringTest() =>
        TimeSpan.Parse(Services.NetTimeDeltaStr(), CultureInfo.InvariantCulture);

    /// <summary>
    /// Benchmark for Python timedelta to .NET TimeSpan via microseconds
    /// </summary>
    [Benchmark]
    public TimeSpan TimeDeltaAsTimeSpanMicrosecondsTest() =>
        TimeSpan.FromMicroseconds(Services.NetTimeDeltaMicroseconds());

    /// <summary>
    /// Benchmark for .NET TimeSpan to Python timedelta via string
    /// </summary>
    [Benchmark]
    public void TimeSpanAsTimeDeltaStringTest() =>
        Services.PythonTimeDeltaStr($"{TimeSpan.FromSeconds(43018.125549):G}");

    /// <summary>
    /// Benchmark for .NET TimeSpan to Python timedelta via microseconds
    /// </summary>
    [Benchmark]
    public void TimeSpanAsTimeDeltaMicrosecondsTest() =>
        Services.PythonTimeMicroseconds(TimeSpan.FromSeconds(43018.125549).Ticks / TimeSpan.TicksPerMicrosecond);

    /// <summary>
    /// Benchmark for Python datetime to .NET DateTimeOffset via string
    /// </summary>
    [Benchmark]
    public DateTimeOffset DateTimeAsDateTimeOffsetStringTest() =>
        DateTimeOffset.Parse(Services.NetDateTimeStr(), CultureInfo.InvariantCulture);

    /// <summary>
    /// Benchmark for Python datetime to .NET DateTimeOffset via microseconds & offset seconds
    /// </summary>
    [Benchmark]
    public DateTimeOffset DateTimeAsDateTimeOffsetMicrosecondsTest()
    {
        var (microseconds, offset) = Services.NetDateTimeOffset();
        return new DateTimeOffset(new DateTime(microseconds * TimeSpan.TicksPerMicrosecond),
            TimeSpan.FromSeconds(offset));
    }

    /// <summary>
    /// Benchmark for .NET DateTimeOffset to Python datetime via string
    /// </summary>
    [Benchmark]
    public void DateTimeOffsetAsDateTimeStringTest() =>
        Services.PythonDateTimeStr($"{DateTimeOffset.Now:O}");

    /// <summary>
    /// Benchmark for .NET DateTimeOffset to Python datetime via microseconds & offset seconds
    /// </summary>
    [Benchmark]
    public void DateTimeOffsetAsDateTimeMicrosecondsTest()
    {
        var dateTimeOffset = DateTimeOffset.Now;
        Services.PythonDateTimeOffset((dateTimeOffset.Ticks / TimeSpan.TicksPerMicrosecond, (long)dateTimeOffset.Offset.TotalSeconds));
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _host.Dispose();
        GC.SuppressFinalize(this);
    }
}
