using System.Text;
using System.Text.Json;
using HevyHeartModels.Internal;
using HevyHeartConsole.Config;
using HevyHeartModels.Hevy.V1;
using HevyHeartModels.Hevy.V2;

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
    /// The authentication token obtained from the login process, used for V2 API authentication.
    /// </summary>
    private string? _authToken;
    
    /// <summary>
    /// The bearer token (access token) obtained from the login process, used for OAuth-style authentication.
    /// </summary>
    private string? _bearerToken;
    
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
        
        // Initialize tokens from config if available
        if (!string.IsNullOrEmpty(config.AuthToken))
        {
            _authToken = config.AuthToken;
        }
    }

    /// <summary>
    /// Authenticates with Hevy using username/email and password to obtain auth tokens.
    /// </summary>
    /// <remarks>
    /// This method replicates the Python login flow:
    /// 1. POST credentials to /login endpoint
    /// 2. Extract auth_token and access_token from response
    /// 3. GET /account to retrieve user information
    /// 4. Store tokens for subsequent V2 API calls
    /// </remarks>
    /// <param name="emailOrUsername">The user's email address or username</param>
    /// <param name="password">The user's password</param>
    /// <returns>True if authentication succeeded, false otherwise</returns>
    public async Task<bool> LoginAsync(string emailOrUsername, string password)
    {
        try
        {
            // Step 1: Login to get auth tokens
            var loginRequest = new
            {
                emailOrUsername = emailOrUsername,
                password = password
            };

            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            //var request = new HttpRequestMessage(HttpMethod.Post, "/login");
            //request.Content = content;

            //request.Headers.Add("x-api-key", "with_great_power");
            //request.Headers.Add("Content-Type", "application/json");
            //request.Headers.Add("accept-encoding", "gzip");

            _httpClient.DefaultRequestHeaders.Add("x-api-key", "with_great_power");
            var loginResponse = await _httpClient.PostAsync("/login", content);
            
            if (loginResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"❌ Login failed with status code: {loginResponse.StatusCode}");
                return false;
            }

            var loginContent = await loginResponse.Content.ReadAsStringAsync();
            var loginData = JsonSerializer.Deserialize<HevyLoginResponse>(loginContent);
            
            if (loginData == null || string.IsNullOrEmpty(loginData.AuthToken))
            {
                Console.WriteLine("❌ Failed to parse login response");
                return false;
            }

            _authToken = loginData.AuthToken;

            // Step 2: Get account information
            using var accountRequest = new HttpRequestMessage(HttpMethod.Get, "/account");
            accountRequest.Headers.Add("auth-token", _authToken);

            var accountResponse = await _httpClient.SendAsync(accountRequest);
            
            if (accountResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"❌ Failed to get account info with status code: {accountResponse.StatusCode}");
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

            // Save authentication data to file for reference
            var authDataFileName = $"hevy_auth_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var authData = new
            {
                AuthToken = _authToken,
                BearerToken = _bearerToken,
                UserId = _userId,
                Username = accountData.Username,
                AuthenticatedAt = DateTime.UtcNow
            };
            await File.WriteAllTextAsync(authDataFileName, 
                JsonSerializer.Serialize(authData, new JsonSerializerOptions { WriteIndented = true }));

            Console.WriteLine($"✅ Successfully authenticated as {accountData.Username} (User ID: {_userId})");
            Console.WriteLine($"   Auth data saved to: {authDataFileName}");
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Login error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Retrieves a paginated list of workouts from the Hevy V1 API.
    /// </summary>
    /// <param name="page">The page number to retrieve (1-based index).</param>
    /// <param name="pageSize">The number of workouts per page. Cannot exceed 10 due to API limitations.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="HevyWorkout"/> objects.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when pageSize exceeds 10.</exception>
    public async Task<List<HevyWorkout>> GetWorkoutsAsync(int page = 1, int pageSize = 10)
    {
        if (pageSize > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), "pageSize cannot exceed 5 due to Hevy API limitations.");
        }

        //https://api.hevyapp.com/v1/workouts?page=1&pageSize=5
        var response = await _httpClient.GetAsync($"/v1/workouts?page={page}&pageSize={pageSize}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        var fileName = $"hevy_workouts_list_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        await File.WriteAllTextAsync(fileName, content);


        var workoutsResponse = JsonSerializer.Deserialize<HevyWorkoutsResponse>(content);
        
        return workoutsResponse?.Workouts ?? new List<HevyWorkout>();
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

        var fileName = $"hevy_workout_{workoutId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        await File.WriteAllTextAsync(fileName, content);

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
        return JsonSerializer.Deserialize<HevyHeartModels.Hevy.V1.GetWorkoutResponse>(content);
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
        // Use instance tokens if available, otherwise fall back to config
        var authToken = _authToken ?? _config.AuthToken;

        if (string.IsNullOrEmpty(authToken))
        {
            throw new InvalidOperationException("Not authenticated with Hevy V2 API. Call LoginAsync first or configure tokens in appsettings.json");
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/workout/{workoutId}");
        
        // Add V2 API specific headers
        request.Headers.Add("X-Api-Key", "klean_kanteen_insulated");
        request.Headers.Add("Auth-Token", authToken);
        request.Headers.Add("Hevy-App-Version", "2.5.6");
        request.Headers.Add("Hevy-App-Build", "1819922");
        request.Headers.Add("Hevy-Platform", "android 36");
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var fileName = $"hevy_workout_v2Hybrid_{workoutId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        await File.WriteAllTextAsync(fileName, content);
        
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
    public async Task<HevyHeartModels.Hevy.V2.GetWorkoutResponse> DeleteWorkoutV2Async(string workoutId)
    {
        // Use instance tokens if available, otherwise fall back to config
        var authToken = _authToken ?? _config.AuthToken;

        if (string.IsNullOrEmpty(authToken))
        {
            throw new InvalidOperationException("Not authenticated with Hevy V2 API. Call LoginAsync first or configure tokens in appsettings.json");
        }

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/workout/{workoutId}");

        // Add V2 API specific headers
        request.Headers.Add("X-Api-Key", "klean_kanteen_insulated");
        //request.Headers.Add("Authorization", $"Bearer {bearerToken}");
        request.Headers.Add("Auth-Token", authToken);
        request.Headers.Add("Hevy-App-Version", "2.5.6");
        request.Headers.Add("Hevy-App-Build", "1819922");
        request.Headers.Add("Hevy-Platform", "android 36");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var fileName = $"hevy_workout_v2Hybrid_{workoutId}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        await File.WriteAllTextAsync(fileName, content);

        return JsonSerializer.Deserialize<HevyHeartModels.Hevy.V2.GetWorkoutResponse>(content);
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
            SupersetId = !string.IsNullOrEmpty(v1Exercise.SupersetId) ? int.Parse(v1Exercise.SupersetId) : null,
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
    /// Updates workout biometrics by creating a new workout with biometric data in the Hevy V2 API.
    /// </summary>
    /// <remarks>
    /// This method creates a new workout POST request that includes the provided biometric data (such as heart rate samples).
    /// It combines exercise data from both V1 and V2 API responses to construct a complete workout payload.
    /// </remarks>
    /// <param name="hevyWorkout">The existing workout response model containing data from both V1 and V2 APIs.</param>
    /// <param name="biometrics">The biometric data to include in the workout, such as heart rate samples and calorie information.</param>
    /// <param name="title">The title for the workout.</param>
    /// <param name="startTime">The start time of the workout.</param>
    /// <param name="endTime">The end time of the workout.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the operation succeeded, false otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the client is not authenticated with the Hevy V2 API.</exception>
    public async Task<bool> UpdateWorkoutBiometricsAsync(GetWorkoutResponseModel hevyWorkout, Biometrics biometrics, string title, DateTime startTime, DateTime endTime)
    {
        // Use instance tokens if available, otherwise fall back to config
        var authToken = _authToken ?? _config.AuthToken;
        
        if (string.IsNullOrEmpty(authToken))
        {
            throw new InvalidOperationException("Not authenticated with Hevy V2 API. Call LoginAsync first or configure tokens in appsettings.json");
        }

        var payload = new PostWorkout()
        {
            ShareToStrava = false,
            Workout = new Workout()
            {
                Title = title,
                StartTime = ((DateTimeOffset)startTime).ToUnixTimeSeconds(),
                EndTime = ((DateTimeOffset)endTime).ToUnixTimeSeconds(),
                Biometrics = biometrics,
                Description = hevyWorkout.GetWorkoutResponseV1.Description,
                AppleWatch = false,
                WearosWatch = true,
                IsPrivate = false,
                IsBiometricsPublic = true,
                WorkoutId = Guid.NewGuid().ToString(),
                Exercises = new List<Exercise>(),
                RoutineId = hevyWorkout.GetWorkoutResponseV1.RoutineId,
                Media = new List<object>()
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
        //request.Headers.Add("Authorization", $"Bearer {bearerToken}");
        request.Headers.Add("Auth-Token", authToken);
        request.Headers.Add("Hevy-App-Version", "2.5.6");
        request.Headers.Add("Hevy-App-Build", "1819922");
        request.Headers.Add("Hevy-Platform", "android 36");
        
        var response = await _httpClient.SendAsync(request);
        return response.IsSuccessStatusCode;
    }
}