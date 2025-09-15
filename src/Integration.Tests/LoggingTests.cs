using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Integration.Tests;

public class LoggingTests(PythonEnvironmentFixture fixture) : IntegrationTestBase(fixture)
{
    // Credit Ben ABT's blog https://benjamin-abt.com/blog/2025/09/03/dotnet-unit-test-ilogger-inmemory/

    private class InMemoryLogger : ILogger
    {
        // Lock used to synchronize writes and snapshot reads. Kept internal to avoid external locking mistakes.
        private readonly object _lock = new();

        public List<(LogLevel Level, EventId Id, Exception? Ex, string Message)> Entries = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
          Func<TState, Exception?, string> formatter)
        {
            // Keep the append atomic with the lock so snapshots can be taken reliably without race conditions.
            lock (_lock)
            {
                Entries.Add(new(logLevel, eventId, exception, formatter(state, exception)));
            }
        }

        public bool Has(LogLevel logLevel, EventId id)
        {
            lock (_lock)
            {
                return Entries.Exists(x => x.Level == logLevel && x.Id == id);
            }
        }

        public IReadOnlyList<(LogLevel Level, EventId Id, Exception? Ex, string Message)> GetEntriesSnapshot()
        {
            lock (_lock)
            {
                return [.. Entries];
            }
        }
    }

    [Fact]
    public void TestLogging_TestDebug()
    {
        var testModule = Env.TestLogging();
        var testLogger = new InMemoryLogger();
        Env.WithPythonLogging(testLogger);
        testModule.TestLogDebug();
        // records
        // Wait a bit
        Thread.Sleep(200); // TODO: Work out a better way of handling this.
        var entries = testLogger.GetEntriesSnapshot();
        Assert.Single(entries);
        Assert.Equal(LogLevel.Debug, entries[0].Level);
        Assert.Equal("Hello world", entries[0].Message);
    }

    // TODO : Test lots of log messages
    // TODO : Test in and out of scope levels
    // TODO : Test named loggers
}
