using System.Text.Json.Serialization;

namespace HevyHeartModels.Hevy.V2;

/// <summary>
/// Response model for Hevy V2 API login endpoint.
/// Contains authentication tokens required for accessing protected API endpoints.
/// </summary>
public class HevyLoginResponse
{
    /// <summary>
    /// Gets or sets the authentication token used for API requests.
    /// This token should be included in the Authorization header for authenticated requests.
    /// </summary>
    [JsonPropertyName("auth_token")]
    public string AuthToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the access token for accessing user-specific resources.
    /// Used in conjunction with auth_token for full API access.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
}