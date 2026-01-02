namespace HevyHeartConsole.Config;

/// <summary>
/// Configuration settings for the local OAuth callback server.
/// Used to handle authorization redirects during Strava authentication flow.
/// </summary>
public class ServerConfig
{
    /// <summary>
    /// Gets or sets the port number for the local callback server.
    /// Must match the port specified in Strava.RedirectUri.
    /// Default: 8080
    /// Change this if port 8080 is already in use by another application.
    /// </summary>
    public int Port { get; set; }
    
    /// <summary>
    /// Gets or sets the hostname for the local callback server.
    /// Default: localhost
    /// Should typically remain as localhost for OAuth security best practices.
    /// </summary>
    public string Host { get; set; } = string.Empty;
}