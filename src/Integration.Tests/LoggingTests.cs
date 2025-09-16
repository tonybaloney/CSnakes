using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Integration.Tests;

public class LoggingTests : IntegrationTestBase
{
    private sealed record LogEntry(LogLevel Level, EventId Id, Exception? Exception, string Message);

    private class ObservableLogger : ILogger
    {
        public event EventHandler<LogEntry>? Logged;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                                Func<TState, Exception?, string> formatter) =>
            Logged?.Invoke(this, new(logLevel, eventId, exception, formatter(state, exception)));
    }

    private readonly ObservableLogger logger;
    private readonly ConcurrentBag<KeyValuePair<int, LogEntry>> entriesBag = new();

    public LoggingTests(PythonEnvironmentFixture fixture) : base(fixture)
    {
        logger = new ObservableLogger();
        var i = 0;
        logger.Logged += (_, entry) =>
            entriesBag.Add(KeyValuePair.Create(Interlocked.Increment(ref i), entry));
    }

    private IEnumerable<LogEntry> Entries =>
        from e in entriesBag.ToArray()
        orderby e.Key
        select e.Value;

    [Fact]
    public async Task TestLogging_TestDebug()
    {
        var testModule = Env.TestLogging();
        await using (Env.WithPythonLogging(logger))
            testModule.TestLogDebug();

        var entry = Assert.Single(Entries);
        Assert.Null(entry.Exception);
        Assert.Equal(LogLevel.Debug, entry.Level);
        Assert.Equal("Hello world", entry.Message);
    }

    [Fact]
    public async Task TestLogging_TestInfo()
    {
        var testModule = Env.TestLogging();
        await using (Env.WithPythonLogging(logger))
            testModule.TestLogInfo();

        var entry = Assert.Single(Entries);
        Assert.Null(entry.Exception);
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.Equal("Hello info world", entry.Message);
    }

    [Fact]
    public async Task TestLogging_TestParamsMessage()
    {
        var testModule = Env.TestLogging();
        await using (Env.WithPythonLogging(logger))
            testModule.TestParamsMessage();

        var entry = Assert.Single(Entries);
        Assert.Null(entry.Exception);
        Assert.Equal(LogLevel.Warning, entry.Level);
        Assert.Equal("Hello this example 3", entry.Message);
    }

    [Fact]
    public async Task TestLogging_TestException()
    {
        var testModule = Env.TestLogging();
        await using (Env.WithPythonLogging(logger))
            testModule.TestLogException();

        var entry = Assert.Single(Entries);
        Assert.NotNull(entry.Exception);
        Assert.Equal(LogLevel.Error, entry.Level);
        var pyRuntimeException = Assert.IsType<PythonRuntimeException>(entry.Exception.InnerException);
        Assert.Equal("division by zero", pyRuntimeException.Message);
        Assert.Equal("ZeroDivisionError", ((PythonInvocationException)entry.Exception).PythonExceptionType);
        Assert.Equal("An error message occurred", entry.Message);
    }

    [Fact]
    public async Task TestLogging_FiftyEntries()
    {
        var testModule = Env.TestLogging();
        await using (Env.WithPythonLogging(logger))
            testModule.TestFiftyEntries(); // Raises 50 log records

        Assert.All(Entries, (entry, i) =>
        {
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Warning, entry.Level);
            Assert.Equal(FormattableString.Invariant($"Error {i}"), entry.Message);
            Assert.Null(entry.Exception);
        });
    }

    [Fact]
    public async Task TestLogging_NamedLogger()
    {
        var testModule = Env.TestLogging();
        await using (Env.WithPythonLogging(logger, "csnakes_logger"))
            testModule.TestNamedLogger("csnakes_logger");

        Assert.NotEmpty(Entries);
    }
    // TODO : Test in and out of scope levels
}
