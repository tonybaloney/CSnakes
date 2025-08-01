using System;
using System.IO;
using CSnakes.Runtime;
using CSnakes.Runtime.Locators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var pythonHomePath = AppContext.BaseDirectory;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
       .WithPython()
       .WithHome(pythonHomePath)
       .FromRedistributable(RedistributablePythonVersion.Python3_10)
       .WithVirtualEnvironment(Path.Combine(pythonHomePath, ".venv"));

var app = builder.Build();

var env = app.Services.GetRequiredService<IPythonEnvironment>();

var hello = env.Hello();
Console.WriteLine(hello.Greetings("World"));

// For more, see: https://tonybaloney.github.io/CSnakes/
