using System.Text;
using System.Text.Json;
using HevyHeartModels.Internal;
using HevyHeartConsole.Config;
using HevyHeartModels.Hevy.V1;
using HevyHeartModels.Hevy.V2;
using HevyHeartModels.Enums;

namespace HevyHeartConsole.Services;

/// <summary>
/// Provides functionality to interact with the Hevy API for workout management and synchronization.
/// Supports both V1 and V2 API endpoints for retrieving, creating, updating, and deleting workouts.
/// </summary>
public class HevyService
{
    /// <summary>
    /// The HTTP client used for making requests to the Hevy API.
    /// </summary>
    private readonly HttpClient _httpClient;
    
    /// <summary>
    /// The configuration settings for the Hevy API, including base URL, API keys, and authentication tokens.
    /// </summary>
    private readonly HevyConfig _config;
    
    /// <summary>
    /// The OAuth access token used as the Bearer token for all authenticated API requests.
    /// </summary>
    private string? _accessToken;

    /// <summary>
    /// The OAuth refresh token used to obtain a new access token when the current one expires.
    /// </summary>
    private string? _refreshToken;

    /// <summary>
    /// The UTC expiry timestamp of the current access token (ISO 8601 format).
    /// </summary>
    private string? _expiresAt;

    /// <summary>
    /// The unique identifier for the authenticated user.
    /// </summary>
    private string? _userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="HevyService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API requests.</param>
    /// <param name="config">The configuration settings containing API keys, base URL, and optional authentication tokens.</param>
    public HevyService(HttpClient httpClient, HevyConfig config)
    {
        _httpClient = httpClient;
        _config = config;
        _httpClient.BaseAddress = new Uri(config.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("api-key", config.ApiKey);
        
        _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        
        // Initialize OAuth tokens from config if available
        if (!string.IsNullOrEmpty(config.AccessToken))
        {
            _accessToken = config.AccessToken;
            _refreshToken = config.RefreshToken;
            _expiresAt = config.ExpiresAt;
        }
    }

    /// <summary>
    /// Authenticates with the Hevy API using an OAuth access token and refresh token.
    /// </summary>
    /// <remarks>
    /// The Hevy username/password login endpoint is no longer functional (broken as of ~Feb 2026).
    /// You must obtain the initial access_token and refresh_token from the Hevy web app login cookie:
    ///   1. Log in at https://app.hevyapp.com in your browser.
    ///   2. Open DevTools (F12) > Application > Cookies > hevy.com.
    ///   3. Find the "auth2.0-token" cookie and URL-decode its value.
    ///   4. Extract "access_token", "refresh_token", and "expires_at" from the JSON value.
    ///   5. Save these to appsettings.json under Hevy:AccessToken, Hevy:RefreshToken, Hevy:ExpiresAt.
    ///
    /// This method will automatically refresh expired tokens using the refresh endpoint.
    /// </remarks>
    /// <param name="accessToken">The OAuth access token from the Hevy browser cookie.</param>
    /// <param name="refreshToken">The OAuth refresh token from the Hevy browser cookie.</param>
    /// <returns>True if authentication succeeded (or tokens refreshed successfully), false otherwise.</returns>
    public async Task<bool> LoginAsync(string accessToken, string refreshToken)
    {
        try
        {
            _accessToken = accessToken;
            _refreshToken = refreshToken;

            // Step 1: Try to use the token as-is first (it may still be valid from the browser cookie)
            using var accountRequest = new HttpRequestMessage(HttpMethod.Get, "/account");
            accountRequest.Headers.Add("x-api-key", "with_great_power");
            accountRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

            var accountResponse = await _httpClient.SendAsync(accountRequest);

            if (accountResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                accountResponse.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                // Token is expired — try refreshing
                Console.WriteLine($"⚠️  Access token rejected ({accountResponse.StatusCode}), attempting refresh...");
                var refreshed = await RefreshTokensAsync();
                if (!refreshed)
                {
                    Console.WriteLine("❌ Token refresh failed. Obtain fresh tokens from the Hevy web app cookie.");
                    return false;
                }

                // Retry account fetch with the new token
                using var retryRequest = new HttpRequestMessage(HttpMethod.Get, "/account");
                retryRequest.Headers.Add("x-api-key", "with_great_power");
                retryRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
                accountResponse = await _httpClient.SendAsync(retryRequest);
            }

            if (accountResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var body = await accountResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Failed to get account info: {accountResponse.StatusCode} — {body}");
                return false;
            }

            var accountContent = await accountResponse.Content.ReadAsStringAsync();
            var accountData = JsonSerializer.Deserialize<HevyAccountResponse>(accountContent);

            if (accountData == null)
            {
                Console.WriteLine("❌ Failed to parse account response");
                return false;
            }

            _userId = accountData.Id;

            Console.WriteLine($"✅ Successfully authenticated as {accountData.Username} (User ID: {_userId})");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Login error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Logs out the current user by clearing all stored authentication tokens and user information.
    /// </summary>
    public void Logout()
    {
        _accessToken = null;
        _refreshToken = null;
        _expiresAt = null;
        _userId = null;
        Console.WriteLine("✅ Logged out successfully. All authentication tokens cleared.");
    }

    /// <summary>
    /// Refreshes the OAuth access token using the stored refresh token.
    /// </summary>
    /// <remarks>
    /// Calls POST https://api.hevyapp.com/auth/refresh_token with the current Bearer access token
    /// and a body of {"refresh_token": "..."}.
    /// On success, updates the stored access_token, refresh_token, and expires_at.
    /// </remarks>
    /// <returns>True if the tokens were refreshed successfully, false otherwise.</returns>
    public async Task<bool> RefreshTokensAsync()
    {
        if (string.IsNullOrEmpty(_accessToken) || string.IsNullOrEmpty(_refreshToken))
        {
            Console.WriteLine("❌ Cannot refresh: no access_token or refresh_token available.");
            return false;
        }

        try
        {
            var body = JsonSerializer.Serialize(new { refresh_token = _refreshToken });
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, "/auth/refresh_token");
            request.Content = content;
            request.Headers.Add("x-api-key", "with_great_power");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _httpClient.SendAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Token refresh failed: {response.StatusCode} — {errorBody}");
                return false;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            _accessToken = root.GetProperty("access_token").GetString();
            _refreshToken = root.GetProperty("refresh_token").GetString();
            _expiresAt = root.GetProperty("expires_at").GetString();

            Console.WriteLine($"✅ Tokens refreshed. New expiry: {_expiresAt}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Token refresh error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Returns the current valid access token, refreshing it first if it is expired.
    /// </summary>
    private async Task<string> GetValidAccessTokenAsync()
    {
        if (string.IsNullOrEmpty(_accessToken))
            throw new InvalidOperationException("Not authenticated. Call LoginAsync first or configure AccessToken/RefreshToken in appsettings.json.");

        // Refresh if expired (with 60-second buffer)
        if (!string.IsNullOrEmpty(_expiresAt) &&
            DateTimeOffset.TryParse(_expiresAt, out var expiry) &&
            expiry <= DateTimeOffset.UtcNow.AddSeconds(60))
        {
            Console.WriteLine("Access token expired or nearly expired — refreshing...");
            var refreshed = await RefreshTokensAsync();
            if (!refreshed)
                throw new InvalidOperationException("Access token expired and refresh failed. Re-obtain tokens from the Hevy web app.");
        }

        return _accessToken!;
    }

    /// <summary>
    /// Retrieves a paginated list of workouts from the Hevy V1 API.
    /// </summary>
    /// <param name="page">The page number to retrieve (1-based index).</param>
    /// <param name="pageSize">The number of workouts per page. Cannot exceed 10 due to API limitations.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="HevyWorkout"/> objects.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when pageSize exceeds 10.</exception>
    public async Task<List<HevyWorkout>> GetWorkoutsAsync(int page = 1, int pageSize = 10, int maxResults = 100)
    {
        if (pageSize > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "pageSize cannot exceed 20 due to Hevy API limitations.");
        }

        var last20Workouts = new List<HevyWorkout>();
        var currentPage = page;
        int totalPages = int.MaxValue;

        while (currentPage <= totalPages && last20Workouts.Count < maxResults)
        {
            var response = await _httpClient.GetAsync($"/v1/workouts?page={currentPage}&pageSize={pageSize}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

            var workoutsResponse = JsonSerializer.Deserialize<HevyWorkoutsResponse>(content);

            if (workoutsResponse == null || !workoutsResponse.Workouts.Any())
            {
                break;
            }

            // Update total pages on first iteration
            if (currentPage == page)
            {
                totalPages = workoutsResponse.PageCount;
            }

            // Add workouts, but respect maxResults limit
            var remainingSlots = maxResults - last20Workouts.Count;
            var workoutsToAdd = workoutsResponse.Workouts.Take(remainingSlots).ToList();
            last20Workouts.AddRange(workoutsToAdd);

            currentPage++;
        }

        return last20Workouts;
    }

    /// <summary>
    /// Retrieves a single workout by its unique identifier from the Hevy V1 API.
    /// </summary>
    /// <param name="workoutId">The unique identifier of the workout to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="HevyWorkout"/> object, or null if not found.</returns>
    public async Task<HevyWorkout?> GetWorkoutAsync(string workoutId)
    {
        var response = await _httpClient.GetAsync($"/v1/workouts/{workoutId}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
#if DEBUG
        var fileName = $"hevy_workout_{workoutId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        await File.WriteAllTextAsync(fileName, content);
#endif
        return JsonSerializer.Deserialize<HevyWorkout>(content);
    }

    /// <summary>
    /// Retrieves workout information using both version 1 and version 2 APIs and combines the results into a single
    /// response model.
    /// </summary>
    /// <remarks>Use this method when you need a unified view of workout data aggregated from multiple API
    /// versions. The returned model includes information from both sources, which may be useful for compatibility or
    /// comparison purposes.</remarks>
    /// <param name="workoutId">The unique identifier of the workout to retrieve. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a combined response model with data
    /// from both API versions for the specified workout.</returns>
    public async Task<GetWorkoutResponseModel> GetWorkoutResponseHybrid(string workoutId)
    {
        var v1Task = await GetWorkoutV1Async(workoutId);
        var v2Task = await GetWorkoutV2Async(workoutId);

        var returnResult = new GetWorkoutResponseModel(v1Task, v2Task);

        return returnResult;
    }

    /// <summary>
    /// Retrieves the details of a workout by its unique identifier using the v1 API.
    /// </summary>
    /// <remarks>The workout data is also saved to a local JSON file named using the workout ID and the
    /// current timestamp. The method throws an exception if the HTTP request is unsuccessful.</remarks>
    /// <param name="workoutId">The unique identifier of the workout to retrieve. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="HevyHeartModels.V1.GetWorkoutResponse"/>
    /// object with the workout details.</returns>
    public async Task<HevyHeartModels.Hevy.V1.GetWorkoutResponse> GetWorkoutV1Async(string workoutId)
    {
        var response = await _httpClient.GetAsync($"/v1/workouts/{workoutId}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
#if DEBUG
        var fileName = $"hevy_workout_{workoutId}_{DateTime.Now:yyyyMMdd_HHmms}.json";
        await File.WriteAllTextAsync(fileName, content);
#endif
        try
        {
            var workoutResponse = JsonSerializer.Deserialize<HevyHeartModels.Hevy.V1.GetWorkoutResponse>(content);
            return workoutResponse;
        }
        catch (Exception err)
        {
            throw;
        }
    }

    /// <summary>
    /// Retrieves the details of a workout by its unique identifier using the v2 API.
    /// </summary>
    /// <remarks>The workout data is also saved to a local JSON file named using the workout ID and the
    /// current timestamp. The method requires V2 API authentication headers including Bearer token and Auth token.</remarks>
    /// <param name="workoutId">The unique identifier of the workout to retrieve. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a V2 GetWorkoutResponse
    /// object with the workout details.</returns>
    public async Task<HevyHeartModels.Hevy.V2.GetWorkoutResponse> GetWorkoutV2Async(string workoutId)
    {
        var accessToken = await GetValidAccessTokenAsync();

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/workout/{workoutId}");
        
        // Add V2 API specific headers
        request.Headers.Add("X-Api-Key", "klean_kanteen_insulated");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("Hevy-App-Version", "2.5.6");
        request.Headers.Add("Hevy-App-Build", "1819922");
        request.Headers.Add("Hevy-Platform", "android 36");
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
#if DEBUG
        var fileName = $"hevy_workout_v2Hybrid_{workoutId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        await File.WriteAllTextAsync(fileName, content);
#endif
        return JsonSerializer.Deserialize<HevyHeartModels.Hevy.V2.GetWorkoutResponse>(content);
    }


    /// <summary>
    /// Deletes a workout from the Hevy V2 API by its unique identifier.
    /// </summary>
    /// <remarks>The deleted workout's details are saved to a local JSON file for reference. This method
    /// requires valid authentication tokens to be present before it is called.</remarks>
    /// <param name="workoutId">The unique identifier of the workout to delete. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a GetWorkoutResponse object with
    /// details of the deleted workout.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the client is not authenticated with the Hevy V2 API. Ensure that authentication tokens are set by
    /// calling LoginAsync or configuring them in the application settings.</exception>
    public async Task<bool> DeleteWorkoutV2Async(string workoutId)
    {
        var accessToken = await GetValidAccessTokenAsync();

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/workout/{workoutId}");

        // Add V2 API specific headers
        request.Headers.Add("X-Api-Key", "klean_kanteen_insulated");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("Hevy-App-Version", "2.5.6");
        request.Headers.Add("Hevy-App-Build", "1819922");
        request.Headers.Add("Hevy-Platform", "android 36");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
#if DEBUG
        var fileName = $"hevy_workout_v2Hybrid_{workoutId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        await File.WriteAllTextAsync(fileName, content);
#endif
        return true;
    }

    /// <summary>
    /// Builds a V2 Exercise object by combining data from both V1 and V2 API exercise models.
    /// </summary>
    /// <param name="v2Exercise">The V2 API exercise model containing REST seconds and other V2-specific data.</param>
    /// <param name="v1Exercise">The V1 API exercise model containing primary exercise information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a combined <see cref="Exercise"/> object.</returns>
    public async Task<Exercise> BuildExerciseFromHybridModels(GetExercise v2Exercise, V1Exercise v1Exercise)
    {
        var exercise = new HevyHeartModels.Hevy.V2.Exercise
        {
            ExerciseTemplateId = v1Exercise.ExerciseTemplateId,
            Title = v1Exercise.Title,
            Notes = v1Exercise.Notes,
            RestTimerSeconds = v2Exercise.RestSeconds,
            VolumeDoublingEnabled = v2Exercise.VolumeDoublingEnabled,
            SupersetId = v1Exercise.SupersetId.HasValue ? v1Exercise.SupersetId.Value : null,
            Sets = new List<Set>()
        };

        // Build sets using V1 data primarily, with V2 for completed_at
        for (int i = 0; i < v1Exercise.Sets.Count; i++)
        {
            var v1Set = v1Exercise.Sets[i];
            var v2Set = i < v2Exercise.Sets.Count ? v2Exercise.Sets[i] : null;

            var set = new Set
            {
                Index = v1Set.Index,
                Type = v1Set.Type,
                WeightKg = v1Set.WeightKg ?? 0,
                Reps = v1Set.Reps,
                DistanceMeters = v1Set.DistanceMeters ?? 0,
                DurationSeconds = v1Set.DurationSeconds,
                Rpe = v1Set.Rpe,
                CompletedAt = v2Set?.CompletedAt ?? string.Empty
            };

            exercise.Sets.Add(set);
        }

        return exercise;
    }

    /// <summary>
    /// Updates workout biometrics by deleting the existing Hevy workout and recreating it with biometric data
    /// using the same workout ID, via the Hevy V2 API.
    /// </summary>
    /// <remarks>
    /// The original workout is deleted first so that its GUID can be reused for the recreated workout.
    /// If deletion fails, an <see cref="InvalidOperationException"/> is thrown and no recreation is attempted.
    /// It combines exercise data from both V1 and V2 API responses to construct a complete workout payload.
    /// </remarks>
    /// <param name="hevyWorkout">The existing workout response model containing data from both V1 and V2 APIs.</param>
    /// <param name="biometrics">The biometric data to include in the workout, such as heart rate samples and calorie information.</param>
    /// <param name="title">The title for the workout.</param>
    /// <param name="startTime">The start time of the workout.</param>
    /// <param name="endTime">The end time of the workout.</param>
    /// <param name="watchType">The type of watch used to record the biometric data (Apple Watch, WearOS, or None).</param>
    /// <param name="shareToStrava">
    /// When <c>true</c>, the recreated workout will be shared to Strava via Hevy's built-in integration.
    /// Set to <c>true</c> only after the original Strava activity has been deleted to avoid duplicates.
    /// </param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the recreation succeeded, false otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the client is not authenticated with the Hevy V2 API, or if the original workout could not be deleted.</exception>
    public async Task<bool> UpdateWorkoutBiometricsAsync(GetWorkoutResponseModel hevyWorkout, Biometrics biometrics, string title, DateTime startTime, DateTime endTime, WatchType watchType = WatchType.None, bool shareToStrava = false)
    {
        var workoutId = hevyWorkout.GetWorkoutResponseV1.Id;

        // Delete the original workout first so its GUID can be reused in the recreated workout.
        var deleted = await DeleteWorkoutV2Async(workoutId);
        if (!deleted)
            throw new InvalidOperationException($"Failed to delete original workout (ID: {workoutId}). Recreation aborted.");

        var accessToken = await GetValidAccessTokenAsync();

        var payload = new PostWorkout()
        {
            ShareToStrava = shareToStrava,
            Workout = new Workout()
            {
                Title = title,
                StartTime = ((DateTimeOffset)startTime).ToUnixTimeSeconds(),
                EndTime = ((DateTimeOffset)endTime).ToUnixTimeSeconds(),
                Biometrics = biometrics,
                Description = hevyWorkout.GetWorkoutResponseV1.Description,
                AppleWatch = watchType == WatchType.AppleWatch,
                WearosWatch = watchType == WatchType.WearOS,
                IsPrivate = false,
                IsBiometricsPublic = true,
                WorkoutId = hevyWorkout.GetWorkoutResponseV1.Id,
                Exercises = new List<Exercise>(),
                RoutineId = hevyWorkout.GetWorkoutResponseV1.RoutineId,
                Media = new List<object>(),
                TrainerProgramId = hevyWorkout.GetWorkoutResponseV2.TrainerProgramId
            }
        };

        foreach (var exercise in hevyWorkout.GetWorkoutResponseV1.Exercises)
        {
            var v2Exercise = hevyWorkout.GetWorkoutResponseV2.Exercises
                .FirstOrDefault(e => e.ExerciseTemplateId == exercise.ExerciseTemplateId && e.Title == exercise.Title);
            if (v2Exercise != null)
            {
                var builtExercise = await BuildExerciseFromHybridModels(v2Exercise, exercise);
                payload.Workout.Exercises.Add(builtExercise);
            }
        }

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = false });
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, "/v2/workout");
        request.Content = content;
        
        // Add V2 API specific headers
        request.Headers.Add("X-Api-Key", "klean_kanteen_insulated");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.Add("Hevy-App-Version", "2.5.6");
        request.Headers.Add("Hevy-App-Build", "1819922");
        request.Headers.Add("Hevy-Platform", "android 36");
        
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}