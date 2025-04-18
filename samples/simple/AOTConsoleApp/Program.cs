using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        var home = Path.Join(Environment.CurrentDirectory);
        services
            .WithPython()
            .WithHome(home)
            .FromRedistributable("3.12");
    });

var app = builder.Build();

var env = app.Services.GetRequiredService<IPythonEnvironment>();

RunQuickDemo(env);

static void RunQuickDemo(IPythonEnvironment env)
{
    var quickDemo = env.AotDemo();
    foreach (var thing in quickDemo.CoolThings())
    {
        Console.WriteLine(thing + " is cool!");
    }
    Console.WriteLine();
}
