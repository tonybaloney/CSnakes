using System;
using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

_ = builder.Services
           .WithPython()
           .WithHome(AppContext.BaseDirectory)
           .FromRedistributable();

using var app = builder.Build();
var env = app.Services.GetRequiredService<IPythonEnvironment>();

var module = env.Demo();

var greeting = module.HelloWorld("World");
Console.WriteLine(greeting);

var sum = module.AddNumbers(3, 4);
Console.WriteLine($"3 + 4 = {sum}");
