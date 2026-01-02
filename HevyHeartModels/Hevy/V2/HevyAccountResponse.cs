using System.Text.Json.Serialization;

namespace HevyHeartModels.Hevy.V2;

/// <summary>
/// Response model for Hevy V2 API account information endpoint.
/// Contains basic user account details including identification and profile information.
/// </summary>
public class HevyAccountResponse
{
    /// <summary>
    /// Gets or sets the unique identifier for the user account.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the username for the account.
    /// This is the public display name visible to other users.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the email address associated with the account.
    /// </summary>
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the URL to the user's profile picture.
    /// Null if no profile picture has been set.
    /// </summary>
    [JsonPropertyName("profile_pic")]
    public string? ProfilePic { get; set; }
}