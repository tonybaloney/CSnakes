using BenchmarkDotNet.Attributes;
using CSnakes.Runtime;
using System.Globalization;

namespace Profile;

public class DatetimeBenchmarks : BaseBenchmark
{
    private IDatetimeBenchmarks? mod;

    [GlobalSetup]
    public void Setup()
    {
        mod = Env.DatetimeBenchmarks();
    }

    /// <summary>
    /// Benchmark for Python uuid to .NET Guid letting .NET convert to big-endian
    /// </summary>
    [Benchmark]
    public Guid GuidAsBigEndianUuidTest() =>
        new(mod!.NetGuidBigEndian(), true);

    /// <summary>
    /// Benchmark for Python uuid to .NET Guid letting Python convert from little-endian
    /// </summary>
    [Benchmark]
    public Guid GuidAsLittleEndianUuidTest() =>
        new(mod!.NetGuidLittleEndian());

    /// <summary>
    /// Benchmark for Python uuid to .NET Guid via string
    /// </summary>
    [Benchmark]
    public Guid GuidAsStringUuidTest() =>
        new(mod!.NetGuidStr());

    /// <summary>
    /// Benchmark for .NET Guid to Python uuid letting .NET convert to big-endian
    /// </summary>
    [Benchmark]
    public void UuidAsBigEndianGuidTest() =>
        mod!.PythonUuidBigEndian(Guid.NewGuid().ToByteArray(true));

    /// <summary>
    /// Benchmark for .NET Guid to Python uuid letting Python convert from little-endian
    /// </summary>
    [Benchmark]
    public void UuidAsLittleEndianGuidTest() =>
        mod!.PythonUuidLittleEndian(Guid.NewGuid().ToByteArray());

    /// <summary>
    /// Benchmark for .NET Guid to Python uuid via string
    /// </summary>
    [Benchmark]
    public void UuidAsStringGuidTest() =>
        mod!.PythonUuidStr(Guid.NewGuid().ToString());

    /// <summary>
    /// Benchmark for .NET DateOnly to Python date via string
    /// </summary>
    [Benchmark]
    public void DateOnlyAsDateStringTest() =>
        mod!.PythonDateStr($"{DateOnly.FromDateTime(DateTime.Now):O}");

    /// <summary>
    /// Benchmark for .NET DateOnly to Python date letting Python convert to DayNumber
    /// </summary>
    [Benchmark]
    public void DateOnlyAsDateOrdinalTest() =>
        mod!.PythonDateOrdinal(DateOnly.FromDateTime(DateTime.Now).DayNumber);

    /// <summary>
    /// Benchmark for Python date to .NET DateOnly via string
    /// </summary>
    [Benchmark]
    public DateOnly DateAsDateOnlyStringTest() =>
        DateOnly.Parse(mod!.NetDateStr(), CultureInfo.InvariantCulture);

    /// <summary>
    /// Benchmark for Python date to .NET DateOnly letting Python convert to DayNumber
    /// </summary>
    [Benchmark]
    public DateOnly DateAsDateOnlyOrdinalTest() =>
        DateOnly.FromDayNumber((int)mod!.NetDateOrdinal());

    /// <summary>
    /// Benchmark for Python time to .NET TimeOnly via string
    /// </summary>
    [Benchmark]
    public TimeOnly TimeAsTimeOnlyStringTest() =>
        TimeOnly.Parse(mod!.NetTimeStr(), CultureInfo.InvariantCulture);

    /// <summary>
    /// Benchmark for Python time to .NET TimeOnly via microseconds
    /// </summary>
    [Benchmark]
    public TimeOnly TimeAsTimeOnlyMicrosecondsTest() =>
        TimeOnly.FromTimeSpan(TimeSpan.FromMicroseconds(mod!.NetTimeMicroseconds()));

    /// <summary>
    /// Benchmark for .NET TimeOnly to Python time via string
    /// </summary>
    [Benchmark]
    public void TimeOnlyAsTimeStringTest() =>
        mod!.PythonTimeStr($"{TimeOnly.FromDateTime(DateTime.Now):O}");

    /// <summary>
    /// Benchmark for .NET TimeOnly to Python time via microseconds
    /// </summary>
    [Benchmark]
    public void TimeOnlyAsTimeMicrosecondsTest() =>
        mod!.PythonTimeMicroseconds(TimeOnly.FromDateTime(DateTime.Now).Ticks / TimeSpan.TicksPerMicrosecond);

    /// <summary>
    /// Benchmark for Python timedelta to .NET TimeSpan via string
    /// </summary>
    [Benchmark]
    public TimeSpan TimeDeltaAsTimeSpanStringTest() =>
        TimeSpan.Parse(mod!.NetTimeDeltaStr(), CultureInfo.InvariantCulture);

    /// <summary>
    /// Benchmark for Python timedelta to .NET TimeSpan via microseconds
    /// </summary>
    [Benchmark]
    public TimeSpan TimeDeltaAsTimeSpanMicrosecondsTest() =>
        TimeSpan.FromMicroseconds(mod!.NetTimeDeltaMicroseconds());

    /// <summary>
    /// Benchmark for .NET TimeSpan to Python timedelta via string
    /// </summary>
    [Benchmark]
    public void TimeSpanAsTimeDeltaStringTest() =>
        mod!.PythonTimeDeltaStr($"{TimeSpan.FromSeconds(43018.125549):G}");

    /// <summary>
    /// Benchmark for .NET TimeSpan to Python timedelta via microseconds
    /// </summary>
    [Benchmark]
    public void TimeSpanAsTimeDeltaMicrosecondsTest() =>
        mod!.PythonTimeMicroseconds(TimeSpan.FromSeconds(43018.125549).Ticks / TimeSpan.TicksPerMicrosecond);

    /// <summary>
    /// Benchmark for Python datetime to .NET DateTimeOffset via string
    /// </summary>
    [Benchmark]
    public DateTimeOffset DateTimeAsDateTimeOffsetStringTest() =>
        DateTimeOffset.Parse(mod!.NetDateTimeStr(), CultureInfo.InvariantCulture);

    /// <summary>
    /// Benchmark for Python datetime to .NET DateTimeOffset via microseconds & offset seconds
    /// </summary>
    [Benchmark]
    public DateTimeOffset DateTimeAsDateTimeOffsetMicrosecondsTest()
    {
        var (microseconds, offset) = mod!.NetDateTimeOffset();
        return new DateTimeOffset(new DateTime(microseconds * TimeSpan.TicksPerMicrosecond),
            TimeSpan.FromSeconds(offset));
    }

    /// <summary>
    /// Benchmark for .NET DateTimeOffset to Python datetime via string
    /// </summary>
    [Benchmark]
    public void DateTimeOffsetAsDateTimeStringTest() =>
        mod!.PythonDateTimeStr($"{DateTimeOffset.Now:O}");

    /// <summary>
    /// Benchmark for .NET DateTimeOffset to Python datetime via microseconds & offset seconds
    /// </summary>
    [Benchmark]
    public void DateTimeOffsetAsDateTimeMicrosecondsTest()
    {
        var dateTimeOffset = DateTimeOffset.Now;
        mod!.PythonDateTimeOffset((dateTimeOffset.Ticks / TimeSpan.TicksPerMicrosecond, (long)dateTimeOffset.Offset.TotalSeconds));
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        mod?.Dispose();
    }
}
