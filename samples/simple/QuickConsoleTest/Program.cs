﻿using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        var home = FindExamplePythonDependencyFolder(Environment.CurrentDirectory);
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

RunQuickDemo(env);

RunDictionaryDemo(env);

RunKmeansDemo(env);

RunAIDemo(env);

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
    // That was boring, how about scikit-learn
    var kmeansExample = env.KmeansExample();
    List<(long, long)> data = [
        (1, 2), (1, 4), (1, 0),
        (10, 2), (10, 4), (10, 0)
    ];

    var (centroids, inertia) = kmeansExample.CalculateKmeansInertia(data, 4);
    Console.WriteLine($"KMeans inertia for 4 clusters is {JsonSerializer.Serialize(centroids)}, inertia is {inertia}");
}

static void RunAIDemo(IPythonEnvironment env)
{
    var phi3demo = env.Phi3Demo();
    // var result = phi3demo.Phi3InferenceDemo("What kind of food is Brie?");
    // Console.WriteLine(result);
}

static string FindExamplePythonDependencyFolder(string startDirectory)
{
    var currentDirectory = startDirectory;

    while (true)
    {
        var potentialHome = Path.Combine(currentDirectory, "ExamplePythonDependency");
        if (Directory.Exists(potentialHome))
        {
            return potentialHome;
        }

        var parentDirectory = Directory.GetParent(currentDirectory);
        if (parentDirectory == null)
        {
            throw new DirectoryNotFoundException("The 'ExamplePythonDependency' folder was not found in any parent directory.");
        }

        currentDirectory = parentDirectory.FullName;
    }
}