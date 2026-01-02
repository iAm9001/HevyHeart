namespace HevyHeartConsole.Config;

/// <summary>
/// Represents the root application configuration loaded from appsettings.json.
/// Contains all settings required for Strava and Hevy API integration, as well as local server settings.
/// </summary>
public class AppConfig
{
    /// <summary>
    /// Gets or sets the Strava API configuration.
    /// Includes OAuth credentials, redirect URI, and API permissions scope.
    /// </summary>
    public StravaConfig Strava { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the Hevy API configuration.
    /// Includes API key, authentication tokens, and user credentials.
    /// </summary>
    public HevyConfig Hevy { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the local callback server configuration.
    /// Used for handling OAuth redirects during Strava authentication.
    /// </summary>
    public ServerConfig Server { get; set; } = new();
}