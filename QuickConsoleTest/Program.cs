using Python.Generated;
using PythonEnvironments;
using System.Text.Json;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
var builder = new PythonEnvironment(
    Environment.GetEnvironmentVariable("USERPROFILE") + "\\.nuget\\packages\\python\\3.12.4\\tools",
    "3.12.4")
    .WithVirtualEnvironment(Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "ExamplePythonDependency", ".venv"));

using var env = builder.Build(Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "ExamplePythonDependency"));

var quickDemo = env.QuickDemo();
Console.WriteLine(quickDemo.Scream("a", 99));
Console.WriteLine(string.Join(',', quickDemo.ScreamNames(new List<string> { "a", "b", "c" }, 3)));

var dict = env.TypeDemos().ReturnDict();

foreach ((string key, long value) in dict)
{
    Console.WriteLine($"{key} : {value}");
}

env.TypeDemos().TakeDict(dict);
// That was boring, how about scikit-learn
var kmeansExample = env.KmeansExample();
List<Tuple<long, long>> data = [
    new (1, 2), new (1, 4), new (1, 0),
        new (10, 2), new (10, 4), new (10, 0)
];

var interiaResult = kmeansExample.CalculateKmeansInteria(data, 4);
var centroids = JsonSerializer.Serialize(interiaResult.Item1);
var interia = interiaResult.Item2;
Console.WriteLine($"KMeans interia for 4 clusters is {centroids}, interia is {interia}");


var phi3demo = env.Phi3Demo();
// var result = phi3demo.Phi3InferenceDemo("What kind of food is Brie?");
// Console.WriteLine(result);