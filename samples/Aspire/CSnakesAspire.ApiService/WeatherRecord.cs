record WeatherRecord(string City, DateOnly Date, float Precipitation, float TemperatureMinC, float TemperatureMaxC, float Wind, string? Summary)
{
    public float TemperatureMinF => 32.0f + (TemperatureMinC / 0.5556f);
    public float TemperatureMaxF => 32.0f + (TemperatureMaxC / 0.5556f);
}
