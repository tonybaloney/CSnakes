using BenchmarkDotNet.Running;
using Profile;

var summary = BenchmarkRunner.Run<MarshallingBenchmarks>(args: args);
