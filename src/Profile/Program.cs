using BenchmarkDotNet.Running;
using Profile;

BenchmarkSwitcher.FromAssembly(typeof(BaseBenchmark).Assembly)
                 .Run(args);
