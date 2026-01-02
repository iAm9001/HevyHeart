namespace HevyHeartConsole.Config;

/// <summary>
/// Configuration settings for Hevy API integration.
/// Contains API key, authentication tokens, and user credentials for accessing Hevy workout data.
/// </summary>
public class HevyConfig
{
    /// <summary>
    /// Gets or sets the Hevy API key.
    /// Required for accessing the Hevy API. Obtain from Hevy developer portal or support.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the Hevy API base URL.
    /// Default: https://api.hevyapp.com
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the Hevy V2 API authentication token.
    /// Optional. If not provided, the application will authenticate using EmailOrUsername and Password.
    /// Leave as empty string or null for automatic login using credentials.
    /// </summary>
    public string AuthToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the Hevy account email address or username.
    /// Used for authentication when AuthToken is not provided.
    /// Note: If you use Google/Apple sign-in, you can create a password in Hevy settings for API access.
    /// </summary>
    public string EmailOrUsername { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the Hevy account password.
    /// Used for authentication when AuthToken is not provided.
    /// Note: Users who authenticate via Google/Apple can create a password in Hevy settings for API access
    /// while continuing to use social login for normal app usage.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}