using CSnakes.Runtime;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

string pythonVersionWindows = "3.12.4";
string pythonVersionMacOS = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";
string pythonVersionLinux = Environment.GetEnvironmentVariable("PYTHON_VERSION") ?? "3.12";

string home = Path.Join(Environment.CurrentDirectory, "..", "python");
builder.Services.WithPython()
    .FromNuGet(pythonVersionWindows)
    .FromMacOSInstallerLocator(pythonVersionMacOS)
    .FromEnvironmentVariable("Python3_ROOT_DIR", pythonVersionLinux)
    .WithHome(home)
    .WithPipInstaller()
    .WithVirtualEnvironment(Path.Join(home, ".venv"));

builder.Services.AddSingleton(sp => sp.GetRequiredService<IPythonEnvironment>().Weather());

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet("/weatherforecast", (
    [FromServices] IWeather weather,
    [FromServices] ILogger<Program> logger) =>
{
    var rawForecast = weather.GetWeatherForecast(Activity.Current?.TraceId.ToString(), Activity.Current.SpanId.ToString());

    logger.LogInformation("Raw forecast: {RawForecast}", rawForecast);

    var forecast = rawForecast
        .Select(f => new WeatherForecast(DateOnly.Parse(f["date"].As<string>()), f["temperature_c"].As<long>(), f["summary"].As<string>()));
    return forecast;
});

app.MapDefaultEndpoints();

app.Run();

record WeatherForecast(DateOnly Date, long TemperatureC, string? Summary)
{
    public long TemperatureF => 32 + (long)(TemperatureC / 0.5556);
}
