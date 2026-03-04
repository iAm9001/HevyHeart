namespace HevyHeartConsole.Config;

/// <summary>
/// Configuration settings for Hevy API integration.
/// 
/// BREAKING CHANGE (Hevy API ~Feb 2026):
/// The username/password login endpoint is no longer functional.
/// Hevy now uses OAuth-style access/refresh tokens. Obtain initial tokens from the
/// browser cookie "auth2.0-token" after logging in at https://app.hevyapp.com.
/// Use browser DevTools (Application > Cookies) to find access_token and refresh_token.
/// </summary>
public class HevyConfig
{
    /// <summary>
    /// Gets or sets the Hevy API key.
    /// Required for accessing the Hevy API.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the Hevy API base URL.
    /// Default: https://api.hevyapp.com
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OAuth access token for Hevy API authentication.
    /// Obtain from the "auth2.0-token" cookie after logging in to https://app.hevyapp.com.
    /// The application will automatically refresh this token using the RefreshToken when it expires.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the OAuth refresh token used to obtain new access tokens.
    /// Obtain from the "auth2.0-token" cookie after logging in to https://app.hevyapp.com.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the expiry timestamp of the access token (ISO 8601 UTC string).
    /// Example: "2026-03-10T12:00:00.000Z"
    /// The application will auto-refresh before this time.
    /// </summary>
    public string ExpiresAt { get; set; } = string.Empty;
}