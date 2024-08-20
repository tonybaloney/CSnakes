using CSnakes.Runtime;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var pythonBuilder = builder.Services.WithPython();
var home = Path.Join(Environment.CurrentDirectory, "..", "ExamplePythonDependency");
var venv = Path.Join(home, ".venv");
pythonBuilder
    .WithHome(home)
    .WithVirtualEnvironment(venv)
    .FromNuGet("3.12.4")
    .FromMacOSInstallerLocator("3.12")
    .FromEnvironmentVariable("Python3_ROOT_DIR", "3.12")
    // .WithPipInstaller()
    ;

builder.Services.AddSingleton(sp => sp.GetRequiredService<IPythonEnvironment>().QuickDemo());
builder.Services.AddSingleton(sp => sp.GetRequiredService<IPythonEnvironment>().TypeDemos());
builder.Services.AddSingleton(sp => sp.GetRequiredService<IPythonEnvironment>().KmeansExample());

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/quick", (IQuickDemo demo) => demo.Scream("a", 99));
app.MapGet("/quick/names", (IQuickDemo demo) => string.Join(',', demo.ScreamNames(new[] { "a", "b", "c" }, 3)));
app.MapGet("/dict", (ITypeDemos demos) =>
{
    var dict = demos.ReturnDict();
    return JsonSerializer.Serialize(dict);
});
app.MapGet("/dict/take", (ITypeDemos demos) =>
{
    var dict = demos.ReturnDict();
    demos.TakeDict(dict);
    return "OK";
});

app.MapGet("/kmeans", (IKmeansExample kmeans) =>
{
    var data = new List<(long, long)>
    {
        (1, 2), (1, 4), (1, 0),
        (10, 2), (10, 4), (10, 0)
    };
    return kmeans.CalculateKmeansInertia(data, 4);
});

app.Run();
