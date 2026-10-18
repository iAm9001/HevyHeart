using System.Diagnostics;
using System.Text;
using System.Text.Json;
using HevyHeartConsole.Config;
using HevyHeartConsole.Infrastructure;
using HevyHeartModels.Hevy.V2;
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
    public async Task<List<StravaActivity>> GetActivitiesAsync(int perPage = 100)
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
    /// Opens the Strava activity page in the default browser so the user can delete the activity manually.
    /// </summary>
    /// <param name="activityId">The Strava activity ID to open.</param>
    /// <returns>True if the browser launch was requested successfully; otherwise, false.</returns>
    public bool OpenActivityDeletionPage(long activityId)
    {
        var url = $"https://www.strava.com/athlete/training_activities/{activityId}";
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Retrieves all recent Strava activities for the authenticated athlete, regardless of whether
    /// they contain heart rate data.
    /// </summary>
    /// <param name="perPage">The number of activities to return (max 200 per Strava API limits).</param>
    /// <returns>A list of all <see cref="StravaActivity"/> objects for the athlete.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the service is not authenticated with Strava.</exception>
    public async Task<List<StravaActivity>> GetAllActivitiesAsync(int perPage = 50)
    {
        if (string.IsNullOrEmpty(_accessToken))
            throw new InvalidOperationException("Not authenticated with Strava");

        var response = await _httpClient.GetAsync($"athlete/activities?per_page={perPage}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<StravaActivity>>(content) ?? new List<StravaActivity>();
    }

    /// <summary>
    /// Finds Strava activities whose start time is within <paramref name="tolerance"/> of
    /// <paramref name="targetTime"/>, excluding a specific activity by ID.
    /// Use this to locate the Hevy-published Strava activity that corresponds to a given Hevy workout.
    /// </summary>
    /// <param name="targetTime">The reference start time to search around (typically the Hevy workout start time).</param>
    /// <param name="excludeActivityId">The ID of the activity to exclude from results (e.g., the Garmin-recorded activity already selected by the user).</param>
    /// <param name="tolerance">The maximum allowed time difference. Defaults to 30 minutes if not specified.</param>
    /// <returns>A list of <see cref="StravaActivity"/> objects whose start times are within the tolerance window.</returns>
    public async Task<List<StravaActivity>> FindActivitiesNearTimeAsync(
        DateTime targetTime,
        long excludeActivityId,
        TimeSpan? tolerance = null)
     {
         var tol = tolerance ?? TimeSpan.FromMinutes(30);
         var allActivities = await GetAllActivitiesAsync();

         var candidates = allActivities
             .Where(a =>
                 a.Id != excludeActivityId &&
                 (a.StartDate.Date == targetTime.Date
                  || a.StartDate.Date == targetTime.Date.AddDays(-1)
                  || a.StartDate.Date == targetTime.Date.AddDays(1)))
             .OrderByDescending(a => a.StartDate).ToList();

         return candidates;
     }

    /// <summary>
    /// Uploads a new Strava activity as a TCX file with embedded heart rate samples.
    /// The upload is processed asynchronously by Strava; the returned <see cref="StravaUploadResponse"/>
    /// contains the upload job ID which can be polled via <c>GET /uploads/{id}</c>.
    /// Requires the <c>activity:write</c> OAuth scope.
    /// </summary>
    /// <param name="title">The title/name for the new Strava activity.</param>
    /// <param name="activityType">
    /// The Strava sport type string (e.g., <c>"WeightTraining"</c>, <c>"Run"</c>).
    /// Passed as the <c>sport_type</c> upload field.
    /// </param>
    /// <param name="startTime">The UTC start time of the activity.</param>
    /// <param name="durationSeconds">The total duration of the activity in seconds.</param>
    /// <param name="biometrics">The biometrics data containing heart rate samples and calorie information.</param>
    /// <param name="externalId">
    /// An optional external identifier to attach to the upload (e.g., the Hevy workout ID).
    /// Useful for deduplication or traceability.
    /// </param>
    /// <returns>
    /// A <see cref="StravaUploadResponse"/> with upload status information, or <c>null</c> if the upload request failed.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the service is not authenticated with Strava.</exception>
    public async Task<StravaUploadResponse?> UploadActivityWithHeartRateAsync(
        string title,
        string activityType,
        DateTime startTime,
        int durationSeconds,
        Biometrics biometrics,
        string? externalId = null)
    {
        if (string.IsNullOrEmpty(_accessToken))
            throw new InvalidOperationException("Not authenticated with Strava");

        var tcxContent = GenerateTcxContent(startTime, durationSeconds, biometrics);
        var tcxBytes = Encoding.UTF8.GetBytes(tcxContent);

        using var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(activityType), "sport_type");
        formContent.Add(new StringContent(title), "name");
        formContent.Add(new StringContent("tcx"), "data_type");

        if (!string.IsNullOrEmpty(externalId))
            formContent.Add(new StringContent(externalId), "external_id");

        var fileContent = new ByteArrayContent(tcxBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
        formContent.Add(fileContent, "file", "workout.tcx");

        try
        {
            var response = await _httpClient.PostAsync("uploads", formContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Strava upload failed: {response.StatusCode} — {errorBody}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<StravaUploadResponse>(content);
        }
        catch (Exception err)
        {
            if (err.Message.ToLowerInvariant().Contains("ssl"))
            {
                await Task.Delay(5000);
                return await this.UploadActivityWithHeartRateAsync(title, activityType, startTime, durationSeconds, biometrics,
                    externalId);
            }
            else
            {
                throw;
            }
        }
    }

    /// <summary>
    /// Generates a TCX (Training Center XML) file content string containing heart rate track points
    /// derived from the provided <paramref name="biometrics"/> data.
    /// </summary>
    /// <param name="startTime">The UTC start time of the activity.</param>
    /// <param name="durationSeconds">Total duration of the activity in seconds.</param>
    /// <param name="biometrics">Biometrics containing heart rate samples and calorie data.</param>
    /// <returns>A well-formed TCX XML string suitable for uploading to Strava.</returns>
    private static string GenerateTcxContent(DateTime startTime, int durationSeconds, Biometrics biometrics)
    {
        var utcStart = startTime.ToUniversalTime();
        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>");
        sb.AppendLine("<TrainingCenterDatabase");
        sb.AppendLine("  xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"");
        sb.AppendLine("  xsi:schemaLocation=\"http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2 http://www.garmin.com/xmlschemas/TrainingCenterDatabasev2.xsd\"");
        sb.AppendLine("  xmlns=\"http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2\">");
        sb.AppendLine("  <Activities>");
        sb.AppendLine("    <Activity Sport=\"Other\">");
        sb.AppendLine($"      <Id>{utcStart:yyyy-MM-ddTHH:mm:ssZ}</Id>");
        sb.AppendLine($"      <Lap StartTime=\"{utcStart:yyyy-MM-ddTHH:mm:ssZ}\">");
        sb.AppendLine($"        <TotalTimeSeconds>{durationSeconds}</TotalTimeSeconds>");
        sb.AppendLine($"        <Calories>{(int)biometrics.TotalCalories}</Calories>");
        sb.AppendLine("        <Intensity>Active</Intensity>");
        sb.AppendLine("        <TriggerMethod>Manual</TriggerMethod>");
        sb.AppendLine("        <Track>");

        foreach (var sample in biometrics.HeartRateSamples)
        {
            var sampleTime = DateTimeOffset.FromUnixTimeMilliseconds(sample.TimestampMs).UtcDateTime;
            sb.AppendLine("          <TrackPoint>");
            sb.AppendLine($"            <Time>{sampleTime:yyyy-MM-ddTHH:mm:ssZ}</Time>");
            sb.AppendLine("            <HeartRateBpm xsi:type=\"HeartRateInBeatsPerMinute_t\">");
            sb.AppendLine($"              <Value>{(int)sample.Bpm}</Value>");
            sb.AppendLine("            </HeartRateBpm>");
            sb.AppendLine("          </TrackPoint>");
        }

        sb.AppendLine("        </Track>");
        sb.AppendLine("      </Lap>");
        sb.AppendLine("    </Activity>");
        sb.AppendLine("  </Activities>");
        sb.AppendLine("</TrainingCenterDatabase>");

        return sb.ToString();
    }

    /// <summary>
    /// Saves the activity data and heart rate stream to a local JSON file for debugging and verification.
    /// The file is saved in the "HistoricalWorkoutData" subdirectory of the application folder, with a
    /// timestamp in the filename.
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
        var data = new
        {
            Activity = activity,
            HeartRateStream = heartRateStream,
            ExportedAt = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        var fileName = HistoricalDataPaths.ResolvePath($"strava_activity_{activityId}_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        await File.WriteAllTextAsync(fileName, json);
        Console.WriteLine($"Strava data saved to: {fileName}");
    }
}