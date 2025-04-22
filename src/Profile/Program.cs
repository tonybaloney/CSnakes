using BenchmarkDotNet.Running;
using Profile;

var summary = BenchmarkRunner.Run<MarshallingBenchmarks>(args: args);
BenchmarkRunner.Run<AsyncBenchmarks>(args: args);
