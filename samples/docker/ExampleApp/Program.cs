using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
var home = Path.Join(Environment.CurrentDirectory);
builder.Services
    .WithPython()
    .WithHome(home)
    .FromRedistributable("3.12")
    .WithVirtualEnvironment(Path.Join(home, ".venv"))
    .WithPipInstaller(Path.Join(home, "requirements.txt"));

var app = builder.Build();

var env = app.Services.GetRequiredService<IPythonEnvironment>();

RunQuickDemo(env);

static void RunQuickDemo(IPythonEnvironment env)
{
    var quickDemo = env.Demo();
    foreach (var thing in quickDemo.CoolThings())
    {
        Console.WriteLine(thing + " is cool!");
    }
    Console.WriteLine();
}
