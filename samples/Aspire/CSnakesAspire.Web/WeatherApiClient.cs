namespace CSnakesAspire.Web;

public class WeatherApiClient(HttpClient httpClient)
{
    public async Task<WeatherRecord[]> GetWeatherAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        List<WeatherRecord>? forecasts = null;

        await foreach (var forecast in httpClient.GetFromJsonAsAsyncEnumerable<WeatherRecord>("/weatherforecast", cancellationToken))
        {
            if (forecasts?.Count >= maxItems)
            {
                break;
            }
            if (forecast is not null)
            {
                forecasts ??= [];
                forecasts.Add(forecast);
            }
        }

        return forecasts?.ToArray() ?? [];
    }
}

public record WeatherRecord(string City, DateOnly Date, float Precipitation, float TemperatureMinC, float TemperatureMaxC, float Wind, string? Summary)
{
    public float TemperatureMinF => 32.0f + (TemperatureMinC / 0.5556f);
    public float TemperatureMaxF => 32.0f + (TemperatureMaxC / 0.5556f);
}
