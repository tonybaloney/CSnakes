using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
            testModule.TestManyEntries(50);

        Assert.All(Entries, (entry, i) =>
        {
            Assert.Null(entry.Exception);
            Assert.Equal(LogLevel.Warning, entry.Level);
            Assert.Equal(FormattableString.Invariant($"Error {i}"), entry.Message);
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

    [Fact]
    public async Task TestLogging_OldRecordsGetDroppedWhenLoggerIsSlow()
    {
        const int bufferSize = 200;
        const int count = bufferSize + 50;

        using var ready = new Barrier(2);
        using var done = new Barrier(2);
        using var countdown = new CountdownEvent(bufferSize);

        using var timeoutTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutTokenSource.Token, TestContext.Current.CancellationToken);
        var cancellationToken = Debugger.IsAttached ? CancellationToken.None : linkedTokenSource.Token;

        var initialPassCount = 0;

        void OnFirstLogged(object? sender, LogEntry e)
        {
            initialPassCount++;
            logger.Logged -= OnFirstLogged; // Stop blocking new entries

            // Deliberately block the first logged entry to cause the internal buffer to fill up and
            // drop subsequent entries.

            ready.SignalAndWait(cancellationToken);

            // Wait until the test code has logged all entries.

            done.SignalAndWait(cancellationToken);

            // Now the buffer should have filled up & dropped entries, so listen for new entries
            // and signal their arrival (drainage).

            logger.Logged += delegate { countdown.Signal(); };
        }

        logger.Logged += OnFirstLogged;

        var testModule = Env.TestLogging();

        await using (Env.WithPythonLogging(logger))
        {
            // Issue one entry that will cause the logger to block the listening thread.

            testModule.TestManyEntries(1);

            // Wait until the first entry has been processed and the logger is blocking.

            ready.SignalAndWait(cancellationToken);

            // Now issue a lot of entries that will fill up the internal buffer and cause older
            // entries to be dropped.

            testModule.TestManyEntries(count);

            // Signal the logger to stop blocking and allow it to drain the buffer.

            done.SignalAndWait(cancellationToken);

            // Wait until the drainage is complete.

            countdown.Wait(cancellationToken);
        }

        Assert.Equal(1, initialPassCount);
        var entries = Entries.Skip(1).ToArray();
        Assert.Equal(bufferSize, entries.Length);
        Assert.All(entries, entry =>
        {
            Assert.Equal(LogLevel.Warning, entry.Level);
            Assert.Matches(@"^Error [1-9][0-9]*$", entry.Message);
            Assert.Null(entry.Exception);
        });
    }

    // TODO : Test in and out of scope levels
}
