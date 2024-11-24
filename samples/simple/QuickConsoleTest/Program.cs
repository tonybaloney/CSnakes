using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuickConsoleTest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        var home = Path.Join(Environment.CurrentDirectory, "..", "..", "..", "..", "ExamplePythonDependency");
        var venv = Path.Join(home, ".venv");
        services
        .WithPython()
        .WithHome(home)
        .WithVirtualEnvironment(venv)
        .FromNuGet("3.12.4")
        .FromMacOSInstallerLocator("3.12")
        .FromEnvironmentVariable("Python3_ROOT_DIR", "3.12")
        .WithPipInstaller();
    });

var app = builder.Build();

var env = app.Services.GetRequiredService<IPythonEnvironment>();

RunEmbedDemo(env);

//RunQuickDemo(env);

//RunDictionaryDemo(env);

//RunKmeansDemo(env);

//RunAIDemo(env);

static void RunEmbedDemo(IPythonEnvironment env)
{
    using FastEmbedIntegration embeds = new(env);
    List<string> entries = new();
    entries.Add("hello there");
    IReadOnlyList<IReadOnlyList<double>> vectors = embeds.Generate(entries);
    Console.WriteLine(vectors);
}

static void RunQuickDemo(IPythonEnvironment env)
{
    var quickDemo = env.QuickDemo();
    Console.WriteLine(quickDemo.Scream("a", 99));
    Console.WriteLine(string.Join(',', quickDemo.ScreamNames(["a", "b", "c"], 3)));
}

static void RunDictionaryDemo(IPythonEnvironment env)
{
    var dict = env.TypeDemos().ReturnDict();
    foreach ((string key, long value) in dict)
    {
        Console.WriteLine($"{key} : {value}");
    }

    env.TypeDemos().TakeDict(dict);
}

static void RunKmeansDemo(IPythonEnvironment env)
{
    // Get the centroids and inertia of a test matrix from scikit-learn kmeans algorithm
    var kmeansExample = env.KmeansExample();
    List<(long, long)> data = [
        (1, 2), (1, 4), (1, 0),
        (10, 2), (10, 4), (10, 0)
    ];

    var (centroids, inertia)= kmeansExample.CalculateKmeansInertia(data, 4);
    var resultMatrix = centroids.AsReadOnlySpan2D<double>();
    Console.WriteLine($"KMeans inertia is {inertia}, centroids are:");
    for (int i = 0; i < resultMatrix.Height; i++)
    {
        for (var j = 0; j < resultMatrix.Width; j++)
        {
            Console.Write(resultMatrix[i, j].ToString().PadLeft(10));
        }
        Console.Write("\n");
    }
}

static void RunAIDemo(IPythonEnvironment env)
{
    var phi3demo = env.Phi3Demo();
    // var result = phi3demo.Phi3InferenceDemo("What kind of food is Brie?");
    // Console.WriteLine(result);
}
