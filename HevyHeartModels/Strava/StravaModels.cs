// Strava Models

using System.Text.Json.Serialization;
namespace HevyHeartModels.Strava;

public class StravaActivity
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("start_date")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("elapsed_time")]
    public int ElapsedTime { get; set; }

    [JsonPropertyName("has_heartrate")]
    public bool HasHeartrate { get; set; }

    [JsonPropertyName("average_heartrate")]
    public double? AverageHeartrate { get; set; }

    [JsonPropertyName("max_heartrate")]
    public double? MaxHeartrate { get; set; }
}

public class StravaDetailedActivity : StravaActivity
{
    [JsonPropertyName("calories")]
    public double? Calories { get; set; }
}

public class StravaHeartRateStream
{
    [JsonPropertyName("data")]
    public float[] Data { get; set; } = Array.Empty<float>();

    [JsonPropertyName("series_type")]
    public string SeriesType { get; set; } = string.Empty;

    [JsonPropertyName("original_size")]
    public int OriginalSize { get; set; }

    [JsonPropertyName("resolution")]
    public string Resolution { get; set; } = string.Empty;
}

public class StravaTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_at")]
    public long ExpiresAt { get; set; }

    [JsonPropertyName("athlete")]
    public StravaAthlete Athlete { get; set; } = new();
}

public class StravaAthlete
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("firstname")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastname")]
    public string LastName { get; set; } = string.Empty;
}