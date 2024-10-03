using CSnakes.Runtime;
using CSnakesAspire.ApiService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Diagnostics;
var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.AddNpgsqlDbContext<WeatherDbContext>("weather");

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

if (app.Environment.IsDevelopment())
{
    int count = 0;
    for (var i = 0; i < 10; i++)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
            context.Database.Migrate();

            var weather = scope.ServiceProvider.GetRequiredService<IWeather>();
            weather.SeedDatabase();
            break;
        }
        catch (NpgsqlException)
        {
            // database was probably not ready yet, so we'll wait a moment.
            await Task.Delay(1000);
            count++;
        }
    }
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet("/weatherforecast", (
    [FromServices] IWeather weather,
    [FromServices] ILogger<Program> logger) =>
{
    var rawForecast = weather.GetWeatherForecast(7, Activity.Current?.TraceId.ToString(), Activity.Current?.SpanId.ToString());

    logger.LogInformation("Raw forecast: {RawForecast}", rawForecast);

    var forecast = rawForecast
        .Select(f => new WeatherRecord(
            f["City"].ToString(),
            DateOnly.FromDateTime(DateTime.Parse(f["Date"].ToString())),
            (float)f["Precipitation"].As<double>(),
            (float)f["TemperatureMinC"].As<double>(),
            (float)f["TemperatureMaxC"].As<double>(),
            (float)f["Wind"].As<double>(),
            f["Summary"].ToString()
        ));
    return forecast;
});

app.MapDefaultEndpoints();

app.Run();
