using BenchmarkDotNet.Running;
using Profile;
Console.WriteLine($"System.Private.CoreLib.dll is located at: {typeof(object).Assembly.Location}");
BenchmarkSwitcher.FromAssembly(typeof(BaseBenchmark).Assembly)
                 .Run(args);
