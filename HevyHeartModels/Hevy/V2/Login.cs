﻿using System.Text.Json.Serialization;

namespace HevyHeartModels.Hevy.V2;

public class HevyLoginResponse
{
    [JsonPropertyName("auth_token")]
    public string AuthToken { get; set; } = string.Empty;
    
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
}