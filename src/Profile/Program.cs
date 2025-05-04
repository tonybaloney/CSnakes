using CSnakes;
using CSnakes.Runtime;
using Microsoft.Extensions.Logging;

var env = Python.GetEnvironment(logLevel: LogLevel.Debug);
var mod = env.AsyncBenchmarks();
Console.WriteLine("START");
await mod.AsyncSleepy(1);
Console.WriteLine("END");
