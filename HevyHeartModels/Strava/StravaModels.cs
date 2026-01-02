// Strava Models

using System.Text.Json.Serialization;
namespace HevyHeartModels.Strava;

/// <summary>
/// Represents a Strava activity summary from the Strava API v3.
/// Contains basic activity information including heart rate data availability.
/// </summary>
public class StravaActivity
{
    /// <summary>
    /// Gets or sets the unique identifier for the activity.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the name or title of the activity (e.g., "Morning Run", "Leg Day").
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of activity (e.g., "Run", "Ride", "Workout", "WeightTraining").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the activity started in UTC.
    /// </summary>
    [JsonPropertyName("start_date")]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the elapsed time of the activity in seconds.
    /// Includes time when the timer was paused.
    /// </summary>
    [JsonPropertyName("elapsed_time")]
    public int ElapsedTime { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether heart rate data was recorded for this activity.
    /// </summary>
    [JsonPropertyName("has_heartrate")]
    public bool HasHeartrate { get; set; }

    /// <summary>
    /// Gets or sets the average heart rate in beats per minute (bpm) for the activity.
    /// Null if no heart rate data was recorded.
    /// </summary>
    [JsonPropertyName("average_heartrate")]
    public double? AverageHeartrate { get; set; }

    /// <summary>
    /// Gets or sets the maximum heart rate in beats per minute (bpm) reached during the activity.
    /// Null if no heart rate data was recorded.
    /// </summary>
    [JsonPropertyName("max_heartrate")]
    public double? MaxHeartrate { get; set; }
}

/// <summary>
/// Represents detailed activity information from the Strava API v3.
/// Extends <see cref="StravaActivity"/> with additional metrics like calories burned.
/// </summary>
public class StravaDetailedActivity : StravaActivity
{
    /// <summary>
    /// Gets or sets the estimated total calories burned during the activity.
    /// Null if calorie data is not available or could not be calculated.
    /// </summary>
    [JsonPropertyName("calories")]
    public double? Calories { get; set; }
}

/// <summary>
/// Represents a heart rate data stream from the Strava API v3.
/// Contains time-series heart rate measurements recorded during an activity.
/// </summary>
public class StravaHeartRateStream
{
    /// <summary>
    /// Gets or sets the array of heart rate measurements in beats per minute (bpm).
    /// Each value represents a heart rate sample at a specific point in time during the activity.
    /// </summary>
    [JsonPropertyName("data")]
    public float[] Data { get; set; } = Array.Empty<float>();

    /// <summary>
    /// Gets or sets the type of data series (typically "distance" or "time" for heart rate streams).
    /// Indicates whether data points are indexed by distance or time.
    /// </summary>
    [JsonPropertyName("series_type")]
    public string SeriesType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original number of data points in the stream before any sampling or compression.
    /// </summary>
    [JsonPropertyName("original_size")]
    public int OriginalSize { get; set; }

    /// <summary>
    /// Gets or sets the resolution of the data stream (e.g., "high", "medium", "low").
    /// Indicates the level of detail or sampling rate of the returned data.
    /// </summary>
    [JsonPropertyName("resolution")]
    public string Resolution { get; set; } = string.Empty;
}

/// <summary>
/// Response model for Strava OAuth token exchange endpoint.
/// Contains access tokens and athlete information after successful authentication.
/// </summary>
public class StravaTokenResponse
{
    /// <summary>
    /// Gets or sets the access token used for making authenticated API requests.
    /// Include this token in the Authorization header as a Bearer token.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the refresh token used to obtain a new access token when it expires.
    /// Store this securely for long-term API access.
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Unix timestamp (in seconds) when the access token expires.
    /// After this time, use the refresh token to obtain a new access token.
    /// </summary>
    [JsonPropertyName("expires_at")]
    public long ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets basic information about the authenticated athlete.
    /// </summary>
    [JsonPropertyName("athlete")]
    public StravaAthlete Athlete { get; set; } = new();
}

/// <summary>
/// Represents basic athlete (user) information from the Strava API v3.
/// Included in authentication responses and activity details.
/// </summary>
public class StravaAthlete
{
    /// <summary>
    /// Gets or sets the unique identifier for the athlete.
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the athlete's first name.
    /// </summary>
    [JsonPropertyName("firstname")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the athlete's last name.
    /// </summary>
    [JsonPropertyName("lastname")]
    public string LastName { get; set; } = string.Empty;
}