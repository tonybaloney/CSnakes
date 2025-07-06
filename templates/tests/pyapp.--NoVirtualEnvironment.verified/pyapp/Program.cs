using System;
using System.IO;
using CSnakes.Runtime;
using CSnakes.Runtime.Locators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
       .WithPython()
       .WithHome(AppContext.BaseDirectory)
       .FromRedistributable(RedistributablePythonVersion.Python3_12);

var app = builder.Build();

var env = app.Services.GetRequiredService<IPythonEnvironment>();

var hello = env.Hello();
Console.WriteLine(hello.Greetings("World"));

// For more, see: https://tonybaloney.github.io/CSnakes/
