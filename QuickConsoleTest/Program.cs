using Python.Generated;
using PythonEnvironments;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
using (var env = new PythonEnvironment(
    Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "ExamplePythonDependency"),
    Environment.GetEnvironmentVariable("USERPROFILE") + "\\.nuget\\packages\\python\\3.12.4\\tools",
    "3.12.4").Build())
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
}