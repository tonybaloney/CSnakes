using System;
using System.IO;
using CSnakes.Runtime;
using CSnakes.Runtime.Locators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
#if (!NoVirtualEnvironment)

var pythonHomePath = AppContext.BaseDirectory;
#endif

var builder = Host.CreateApplicationBuilder(args);
builder.Services
       .WithPython()
#if (NoVirtualEnvironment)
       .WithHome(AppContext.BaseDirectory)
#if (UsePipInstaller)
       .FromRedistributable(RedistributablePythonVersion.Python3_12)
       .WithPipInstaller();
#elif (UseUvInstaller)
       .FromRedistributable(RedistributablePythonVersion.Python3_12)
       .WithUvInstaller();
#else
       .FromRedistributable(RedistributablePythonVersion.Python3_12);
#endif
#else
       .WithHome(pythonHomePath)
       .FromRedistributable(RedistributablePythonVersion.Python3_12)
#if (UsePipInstaller)
       .WithVirtualEnvironment(Path.Combine(pythonHomePath, ".venv"))
       .WithPipInstaller();
#elif (UseUvInstaller)
       .WithVirtualEnvironment(Path.Combine(pythonHomePath, ".venv"))
       .WithUvInstaller();
#else
       .WithVirtualEnvironment(Path.Combine(pythonHomePath, ".venv"));
#endif
#endif
#if DEBUG

builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

var app = builder.Build();

var env = app.Services.GetRequiredService<IPythonEnvironment>();

var hello = env.Hello();
Console.WriteLine(hello.Greetings("World"));

// For more, see: https://tonybaloney.github.io/CSnakes/
