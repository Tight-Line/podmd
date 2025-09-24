namespace PodMD.Application.Configuration;

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxRetryCount { get; set; } = 5;
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);
}
