namespace CSnakesAspire.ApiService;

public class Weather
{
    public int Id { get; set; }
    public string City { get; set; } = null!;
    public DateTime Date { get; set; }
    public float Precipitation { get; set; }
    public float TemperatureMinC { get; set; }
    public float TemperatureMaxC { get; set; }
    public float Wind { get; set; }
    public string Summary { get; set; } = null!;
}
