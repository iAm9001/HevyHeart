namespace HevyHeartConsole.Config;

/// <summary>
/// Configuration settings for Strava API integration.
/// Contains OAuth credentials and settings required for authenticating with Strava.
/// </summary>
public class StravaConfig
{
    /// <summary>
    /// Gets or sets the Strava API Client ID.
    /// Obtained from the Strava API application settings at https://www.strava.com/settings/api
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the Strava API Client Secret.
    /// Keep this value secure and never commit it to public repositories.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the OAuth redirect URI for authorization callbacks.
    /// Must match the Authorization Callback Domain configured in your Strava API application.
    /// Default: http://localhost:8080/callback
    /// </summary>
    public string RedirectUri { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the OAuth permission scope requested from Strava.
    /// Default: "read,activity:read_all" to read athlete profile and activity data including heart rate streams.
    /// </summary>
    public string Scope { get; set; } = string.Empty;
}