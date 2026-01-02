using System.Text;
using System.Text.Json;
using HevyHeartConsole.Config;
using HevyHeartModels.Strava;

namespace HevyHeartConsole.Services;

public class StravaService
{
    private readonly HttpClient _httpClient;
    private readonly StravaConfig _config;
    private string? _accessToken;

    public StravaService(HttpClient httpClient, StravaConfig config)
    {
        _httpClient = httpClient;
        _config = config;
        _httpClient.BaseAddress = new Uri("https://www.strava.com/api/v3/");
    }

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

    public async Task<StravaDetailedActivity?> GetActivityAsync(long activityId)
    {
        if (string.IsNullOrEmpty(_accessToken))
            throw new InvalidOperationException("Not authenticated with Strava");

        var response = await _httpClient.GetAsync($"activities/{activityId}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<StravaDetailedActivity>(content);
    }

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

    public async Task SaveActivityDataAsync(long activityId, StravaDetailedActivity activity, StravaHeartRateStream? heartRateStream)
    {
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
    }
}