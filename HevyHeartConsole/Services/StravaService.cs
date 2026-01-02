using System.Text;
using System.Text.Json;
using HevyHeartConsole.Config;
using HevyHeartModels.Strava;

namespace HevyHeartConsole.Services;

/// <summary>
/// Service for interacting with the Strava API v3.
/// Handles OAuth authentication, activity retrieval, and heart rate data extraction.
/// </summary>
public class StravaService
{
    private readonly HttpClient _httpClient;
    private readonly StravaConfig _config;
    private string? _accessToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="StravaService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for API requests.</param>
    /// <param name="config">The Strava configuration containing OAuth credentials and settings.</param>
    public StravaService(HttpClient httpClient, StravaConfig config)
    {
        _httpClient = httpClient;
        _config = config;
        _httpClient.BaseAddress = new Uri("https://www.strava.com/api/v3/");
    }

    /// <summary>
    /// Generates the Strava OAuth authorization URL for user authentication.
    /// The user must visit this URL in a browser to authorize the application.
    /// </summary>
    /// <returns>The complete authorization URL including all required OAuth parameters.</returns>
    public string GetAuthorizationUrl()
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _config.ClientId,
            ["response_type"] = "code",
            ["redirect_uri"] = _config.RedirectUri,
            ["approval_prompt"] = "force",
            ["scope"] = _config.Scope
        };

        var queryString = string.Join("&", parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
        return $"https://www.strava.com/oauth/authorize?{queryString}";
    }

    /// <summary>
    /// Exchanges an OAuth authorization code for an access token.
    /// This completes the OAuth flow after the user has authorized the application.
    /// </summary>
    /// <param name="code">The authorization code received from the OAuth callback.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result is <c>true</c> if the token exchange was successful; otherwise, <c>false</c>.
    /// </returns>
    public async Task<bool> ExchangeCodeForTokenAsync(string code)
    {
        var tokenRequest = new
        {
            client_id = _config.ClientId,
            client_secret = _config.ClientSecret,
            code,
            grant_type = "authorization_code"
        };

        var json = JsonSerializer.Serialize(tokenRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://www.strava.com/oauth/token", content);
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<StravaTokenResponse>(responseContent);
            if (tokenResponse != null)
            {
                _accessToken = tokenResponse.AccessToken;
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Retrieves the authenticated athlete's recent activities from Strava.
    /// Only activities that contain heart rate data are returned.
    /// </summary>
    /// <param name="perPage">The number of activities to retrieve per page. Default is 30.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a list of <see cref="StravaActivity"/> objects that have heart rate data.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the service is not authenticated with Strava.</exception>
    /// <exception cref="HttpRequestException">Thrown if the API request fails.</exception>
    public async Task<List<StravaActivity>> GetActivitiesAsync(int perPage = 30)
    {
        if (string.IsNullOrEmpty(_accessToken))
            throw new InvalidOperationException("Not authenticated with Strava");

        var response = await _httpClient.GetAsync($"athlete/activities?per_page={perPage}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var activities = JsonSerializer.Deserialize<List<StravaActivity>>(content) ?? new List<StravaActivity>();
        
        return activities.Where(a => a.HasHeartrate).ToList();
    }

    /// <summary>
    /// Retrieves detailed information for a specific Strava activity.
    /// </summary>
    /// <param name="activityId">The unique identifier of the activity to retrieve.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the <see cref="StravaDetailedActivity"/> with complete activity information,
    /// or <c>null</c> if the activity cannot be retrieved or deserialized.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the service is not authenticated with Strava.</exception>
    /// <exception cref="HttpRequestException">Thrown if the API request fails.</exception>
    public async Task<StravaDetailedActivity?> GetActivityAsync(long activityId)
    {
        if (string.IsNullOrEmpty(_accessToken))
            throw new InvalidOperationException("Not authenticated with Strava");

        var response = await _httpClient.GetAsync($"activities/{activityId}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<StravaDetailedActivity>(content);
    }

    /// <summary>
    /// Retrieves the heart rate stream data for a specific Strava activity.
    /// The stream contains time-series heart rate measurements recorded during the activity.
    /// </summary>
    /// <param name="activityId">The unique identifier of the activity.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the <see cref="StravaHeartRateStream"/> with all heart rate data points,
    /// or <c>null</c> if no heart rate data is available for the activity.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the service is not authenticated with Strava.</exception>
    /// <exception cref="HttpRequestException">Thrown if the API request fails.</exception>
    public async Task<StravaHeartRateStream?> GetHeartRateStreamAsync(long activityId)
    {
        if (string.IsNullOrEmpty(_accessToken))
            throw new InvalidOperationException("Not authenticated with Strava");

        var response = await _httpClient.GetAsync($"activities/{activityId}/streams?keys=heartrate&key_by_type=true");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var streams = JsonSerializer.Deserialize<Dictionary<string, StravaHeartRateStream>>(content);
        
        return streams?.ContainsKey("heartrate") == true ? streams["heartrate"] : null;
    }

    /// <summary>
    /// Saves the activity data and heart rate stream to a local JSON file for debugging and verification.
    /// The file is saved in the current working directory with a timestamp in the filename.
    /// </summary>
    /// <param name="activityId">The unique identifier of the activity.</param>
    /// <param name="activity">The detailed activity information to save.</param>
    /// <param name="heartRateStream">The heart rate stream data to save (optional).</param>
    /// <returns>A task that represents the asynchronous file write operation.</returns>
    /// <remarks>
    /// The filename format is: strava_activity_{activityId}_{yyyyMMdd_HHmmss}.json
    /// The JSON output is formatted with indentation for readability.
    /// </remarks>
    public async Task SaveActivityDataAsync(long activityId, StravaDetailedActivity activity, StravaHeartRateStream? heartRateStream)
    {
#if DEBUG
        var data = new
        {
            Activity = activity,
            HeartRateStream = heartRateStream,
            ExportedAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        var fileName = $"strava_activity_{activityId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        await File.WriteAllTextAsync(fileName, json);
        Console.WriteLine($"Strava data saved to: {fileName}");
#endif
    }
}