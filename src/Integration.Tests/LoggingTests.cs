using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

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
    private readonly BlockingCollection<LogEntry> entries = new();

    public LoggingTests(PythonEnvironmentFixture fixture) : base(fixture)
    {
        logger = new ObservableLogger();
        logger.Logged += (_, entry) => entries.Add(entry);
    }

    private LogEntry? TryTake() =>
        entries.TryTake(out var entry, millisecondsTimeout: 3_000, TestContext.Current.CancellationToken) ? entry : null;

    [Fact]
    public void TestLogging_TestDebug()
    {
        var testModule = Env.TestLogging();
        using (Env.WithPythonLogging(logger))
        {
            testModule.TestLogDebug();
            var entry = TryTake();
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Debug, entry.Level);
            Assert.Equal("Hello world", entry.Message);
            Assert.Null(TryTake());
        }
    }

    [Fact]
    public void TestLogging_TestInfo()
    {
        var testModule = Env.TestLogging();
        using (Env.WithPythonLogging(logger))
        {
            testModule.TestLogInfo();
            var entry = TryTake();
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Information, entry.Level);
            Assert.Equal("Hello info world", entry.Message);
            Assert.Null(TryTake());
        }
    }

    [Fact]
    public void TestLogging_TestParamsMessage() {
        var testModule = Env.TestLogging();
        using (Env.WithPythonLogging(logger))
        {
            testModule.TestParamsMessage();
            var entry = TryTake();
            Assert.NotNull(entry);
            Assert.Equal(LogLevel.Warning, entry.Level);
            Assert.Equal("Hello this example 3", entry.Message);
            Assert.Null(TryTake());
        }
    }

    [Fact]
    public void TestLogging_TestException()
    {
        var testModule = Env.TestLogging();
        using (Env.WithPythonLogging(logger))
        {
            testModule.TestLogDebug();
            var entry = TryTake();
            Assert.NotNull(entry);
            Assert.NotNull(entry.Exception);
            Assert.Equal(LogLevel.Error, entry.Level);
            Assert.Equal("An error message occurred", entry.Message);
            Assert.Null(TryTake());
        }
    }
    // TODO : Test lots of log messages
    // TODO : Test in and out of scope levels
    // TODO : Test named loggers
}
