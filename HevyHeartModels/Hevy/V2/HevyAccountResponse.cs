using System.Text.Json.Serialization;

namespace HevyHeartModels.Hevy.V2;

public class HevyAccountResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("profile_pic")]
    public string? ProfilePic { get; set; }
}