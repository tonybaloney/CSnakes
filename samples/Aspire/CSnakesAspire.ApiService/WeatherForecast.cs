record WeatherForecast(DateOnly Date, long TemperatureC, string? Summary)
{
    public long TemperatureF => 32 + (long)(TemperatureC / 0.5556);
}
