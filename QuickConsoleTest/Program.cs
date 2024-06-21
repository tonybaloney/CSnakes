using Python.Generated;
using PythonEnvironments;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
using (var env = new PythonEnvironment("C:\\Users\\anthonyshaw\\projects\\Build2024AspireDemo\\ExamplePythonDependency", "3.10").Build())
{
    var quickDemo = env.QuickDemo();
    Console.WriteLine(quickDemo.Scream("a", 99));
    Console.WriteLine(string.Join(',', quickDemo.ScreamNames(new List<string> { "a", "b", "c" }, 3)));
}