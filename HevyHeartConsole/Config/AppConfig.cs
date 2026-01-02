namespace HevyHeartConsole.Config;

public class AppConfig
{
    public StravaConfig Strava { get; set; } = new();
    public HevyConfig Hevy { get; set; } = new();
    public ServerConfig Server { get; set; } = new();
}