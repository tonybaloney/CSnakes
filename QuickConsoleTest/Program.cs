using Python.Generated;
using PythonEnvironments;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
var userProfile = Environment.GetEnvironmentVariable("USERPROFILE");

using (var env = new PythonEnvironment(
    $"{userProfile}\\.nuget\\packages\\python\\3.12.4\\tools",
    "3.12").WithVirtualEnvironment("C:\\Users\\anthonyshaw\\projects\\Build2024AspireDemo\\ExamplePythonDependency\\.venv")
    .Build("C:\\Users\\anthonyshaw\\projects\\Build2024AspireDemo\\ExamplePythonDependency"))
{
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
    Console.WriteLine($"KMeans interia for 4 clusters is {kmeansExample.CalculateKmeansInteria(data, 4)}");
}