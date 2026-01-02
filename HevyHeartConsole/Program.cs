using System.Diagnostics;
using System.Text.Json;
using HevyHeartConsole.Infrastructure;
using HevyHeartConsole.Services;
using HevyHeartModels.Internal;
using HevyHeartConsole.Config;
using HevyHeartModels.Hevy;
using HevyHeartModels.Strava;
using Microsoft.Extensions.Configuration;

namespace HevyHeartConsole;

/// <summary>
/// Main entry point for the HevyHeart console application.
/// Orchestrates the workflow for synchronizing heart rate data from Strava activities to Hevy workouts.
/// </summary>
class Program
{
    private static AppConfig _config = new();
    private static StravaService? _stravaService;
    private static HevyService? _hevyService;

    /// <summary>
    /// Application entry point. Initializes configuration, validates settings, and runs the main workflow.
    /// </summary>
    /// <param name="_">Command line arguments (not used).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    static async Task Main(string[] _)
    {
        Console.WriteLine("=== Hevy Heart Rate Synchronizer ===");
        Console.WriteLine();

        try
        {
            // Load configuration
            LoadConfiguration();

            // Validate configuration
            if (!ValidateConfiguration())
            {
                Console.WriteLine("Please update appsettings.json with your API credentials.");
                return;
            }

            // Initialize services
            var httpClient = new HttpClient();
            _stravaService = new StravaService(httpClient, _config.Strava);
            _hevyService = new HevyService(new HttpClient(), _config.Hevy);

            // Main application flow
            await RunApplicationAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Loads application configuration from appsettings.json.
    /// </summary>
    private static void LoadConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        _config = configuration.Get<AppConfig>() ?? new AppConfig();
        Console.WriteLine("Configuration loaded successfully.");
    }

    /// <summary>
    /// Validates that all required configuration values are present and not using placeholder values.
    /// Checks Strava Client ID, Client Secret, and Hevy API Key. Also validates Hevy credentials if provided.
    /// </summary>
    /// <returns><c>true</c> if all required configuration is valid; otherwise, <c>false</c>.</returns>
    private static bool ValidateConfiguration()
    {
        if (string.IsNullOrEmpty(_config.Strava.ClientId) || _config.Strava.ClientId == "YOUR_STRAVA_CLIENT_ID")
        {
            Console.WriteLine("❌ Strava Client ID not configured");
            return false;
        }

        if (string.IsNullOrEmpty(_config.Strava.ClientSecret) || _config.Strava.ClientSecret == "YOUR_STRAVA_CLIENT_SECRET")
        {
            Console.WriteLine("❌ Strava Client Secret not configured");
            return false;
        }

        if (string.IsNullOrEmpty(_config.Hevy.ApiKey) || _config.Hevy.ApiKey == "YOUR_HEVY_API_KEY")
        {
            Console.WriteLine("❌ Hevy API Key not configured");
            return false;
        }

        // V2 tokens are optional - can be obtained via login
        bool hasV2Tokens = !string.IsNullOrEmpty(_config.Hevy.AuthToken);
        bool hasCredentials = !string.IsNullOrEmpty(_config.Hevy.EmailOrUsername) && 
                             !string.IsNullOrEmpty(_config.Hevy.Password);

        if (!hasV2Tokens && !hasCredentials)
        {
            Console.WriteLine("⚠️  Hevy V2 API tokens or credentials not configured (you will be prompted to login)");
        }

        Console.WriteLine("✅ Configuration validated successfully");
        return true;
    }

    /// <summary>
    /// Executes the main application workflow:
    /// 1. Authenticate with Hevy
    /// 2. Authenticate with Strava
    /// 3. Select a Strava activity with heart rate data
    /// 4. Fetch detailed activity and heart rate stream
    /// 5. Select a Hevy workout to synchronize with
    /// 6. Synchronize heart rate data and optionally update Hevy
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task RunApplicationAsync()
    {
        // Step 0: Authenticate with Hevy if needed
        Console.WriteLine("\n--- Step 0: Hevy Authentication ---");
        await AuthenticateWithHevyAsync();

        // Step 1: Authenticate with Strava
        Console.WriteLine("\n--- Step 1: Strava Authentication ---");
        await AuthenticateWithStravaAsync();

        // Step 2: Get Strava activities
        Console.WriteLine("\n--- Step 2: Fetching Strava Activities ---");
        var stravaActivity = await SelectStravaActivityAsync();
        if (stravaActivity == null) return;

        // Step 3: Get detailed Strava data
        Console.WriteLine("\n--- Step 3: Fetching Strava Activity Details ---");
        var detailedActivity = await _stravaService!.GetActivityAsync(stravaActivity.Id);
        var heartRateStream = await _stravaService.GetHeartRateStreamAsync(stravaActivity.Id);

        if (detailedActivity == null || heartRateStream == null)
        {
            Console.WriteLine("❌ Failed to get detailed activity data or heart rate stream");
            return;
        }

        // Save Strava data for debugging
        await _stravaService.SaveActivityDataAsync(stravaActivity.Id, detailedActivity, heartRateStream);

        // Step 4: Get Hevy workouts
        Console.WriteLine("\n--- Step 4: Fetching Hevy Workouts ---");
        var hevyWorkout = await SelectHevyWorkoutAsync();
        if (hevyWorkout == null) return;

        // Step 5: Synchronize heart rate data
        Console.WriteLine("\n--- Step 5: Synchronizing Heart Rate Data ---");
        await SynchronizeHeartRateAsync(detailedActivity, heartRateStream, hevyWorkout);

        Console.WriteLine("\n✅ Heart rate synchronization completed successfully!");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    /// <summary>
    /// Authenticates with Hevy using configured credentials or by prompting the user.
    /// Checks for existing V2 API tokens or credentials in configuration, 
    /// or prompts for manual login if neither are available.
    /// </summary>
    /// <returns>A task representing the asynchronous authentication operation.</returns>
    /// <exception cref="Exception">Thrown if authentication fails.</exception>
    private static async Task AuthenticateWithHevyAsync()
    {
        // Check if V2 tokens are already configured
        bool hasV2Tokens = !string.IsNullOrEmpty(_config.Hevy.AuthToken);
        
        if (hasV2Tokens)
        {
            Console.WriteLine("✅ Using configured Hevy V2 API tokens from appsettings.json");
            return;
        }

        // Check if credentials are configured
        bool hasCredentials = !string.IsNullOrEmpty(_config.Hevy.EmailOrUsername) && 
                             !string.IsNullOrEmpty(_config.Hevy.Password);

        if (!hasCredentials)
        {
            Console.WriteLine("⚠️  Hevy V2 API tokens not configured in appsettings.json");
            Console.WriteLine("💡 Note: V2 API tokens (BearerToken and AuthToken) are required for some features.");
            Console.WriteLine("   You can either:");
            Console.WriteLine("   1. Add them manually to appsettings.json if you have them");
            Console.WriteLine("   2. Add EmailOrUsername and Password to appsettings.json to login automatically");
            Console.WriteLine("   3. Login now with your Hevy credentials");
            Console.WriteLine();
            Console.Write("Would you like to login now? (y/N): ");
            var response = Console.ReadLine()?.Trim().ToLowerInvariant();
            
            if (response != "y" && response != "yes")
            {
                Console.WriteLine("Continuing without V2 API authentication (some features may be limited)...");
                return;
            }

            Console.Write("Enter Hevy email or username: ");
            var emailOrUsername = Console.ReadLine()?.Trim() ?? "";
            Console.Write("Enter Hevy password: ");
            var password = ReadPassword();
            Console.WriteLine();

            var success = await _hevyService!.LoginAsync(emailOrUsername, password);
            if (!success)
            {
                throw new Exception("Failed to authenticate with Hevy");
            }
        }
        else
        {
            Console.WriteLine("Authenticating with Hevy using configured credentials...");
            var success = await _hevyService!.LoginAsync(_config.Hevy.EmailOrUsername, _config.Hevy.Password);
            
            if (!success)
            {
                throw new Exception("Failed to authenticate with Hevy using configured credentials");
            }
        }
    }

    /// <summary>
    /// Reads a password from console input with masked characters.
    /// Displays asterisks (*) instead of the actual characters typed.
    /// Supports backspace for correction.
    /// </summary>
    /// <returns>The password entered by the user.</returns>
    private static string ReadPassword()
    {
        var password = "";
        ConsoleKey key;
        do
        {
            var keyInfo = Console.ReadKey(intercept: true);
            key = keyInfo.Key;

            if (key == ConsoleKey.Backspace && password.Length > 0)
            {
                Console.Write("\b \b");
                password = password[0..^1];
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                Console.Write("*");
                password += keyInfo.KeyChar;
            }
        } while (key != ConsoleKey.Enter);
        
        return password;
    }

    /// <summary>
    /// Authenticates with Strava using OAuth 2.0 authorization code flow.
    /// Opens a browser for user authorization, starts a local callback server,
    /// and exchanges the authorization code for an access token.
    /// </summary>
    /// <returns>A task representing the asynchronous authentication operation.</returns>
    /// <exception cref="Exception">Thrown if token exchange fails.</exception>
    private static async Task AuthenticateWithStravaAsync()
    {
        var authUrl = _stravaService!.GetAuthorizationUrl();
        
        Console.WriteLine("To authenticate with Strava, you need to:");
        Console.WriteLine("1. A web browser will open with the Strava authorization page");
        Console.WriteLine("2. Log in to Strava and authorize this application");
        Console.WriteLine("3. You'll be redirected back to complete the process");
        Console.WriteLine();
        Console.Write("Press any key to open the authorization page...");
        Console.ReadKey();
        Console.WriteLine();

        // Start callback server
        var callbackServer = new CallbackServer(_config.Strava.RedirectUri);
        var callbackTask = callbackServer.StartAndWaitForCallbackAsync();

        // Open browser
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });
        }
        catch
        {
            Console.WriteLine($"Please manually open this URL in your browser:");
            Console.WriteLine(authUrl);
        }

        // Wait for callback
        try
        {
            var code = await callbackTask;
            var success = await _stravaService.ExchangeCodeForTokenAsync(code);
            
            if (success)
            {
                Console.WriteLine("✅ Successfully authenticated with Strava!");
            }
            else
            {
                throw new Exception("Failed to exchange authorization code for access token");
            }
        }
        finally
        {
            callbackServer.Stop();
        }
    }

    /// <summary>
    /// Displays a list of Strava activities with heart rate data and prompts the user to select one.
    /// Shows activity name, date, type, duration, and average heart rate for each activity.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the selected <see cref="StravaActivity"/>, 
    /// or <c>null</c> if no activities with heart rate data are found.
    /// </returns>
    private static async Task<StravaActivity?> SelectStravaActivityAsync()
    {
        var activities = await _stravaService!.GetActivitiesAsync();
        
        if (!activities.Any())
        {
            Console.WriteLine("❌ No activities with heart rate data found");
            return null;
        }

        Console.WriteLine($"Found {activities.Count} activities with heart rate data:");
        Console.WriteLine();

        for (int i = 0; i < activities.Count; i++)
        {
            var activity = activities[i];
            Console.WriteLine($"{i + 1,2}. {activity.Name}");
            Console.WriteLine($"     Date: {activity.StartDate:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"     Type: {activity.Type}");
            Console.WriteLine($"     Duration: {TimeSpan.FromSeconds(activity.ElapsedTime):hh\\:mm\\:ss}");
            if (activity.AverageHeartrate.HasValue)
                Console.WriteLine($"     Avg HR: {activity.AverageHeartrate:F0} bpm");
            Console.WriteLine();
        }

        while (true)
        {
            Console.Write($"Select an activity (1-{activities.Count}): ");
            if (int.TryParse(Console.ReadLine(), out int selection) && 
                selection >= 1 && selection <= activities.Count)
            {
                var selected = activities[selection - 1];
                Console.WriteLine($"✅ Selected: {selected.Name}");
                return selected;
            }
            Console.WriteLine("Invalid selection. Please try again.");
        }
    }

    /// <summary>
    /// Displays a list of Hevy workouts and prompts the user to select one.
    /// Shows workout title, date, duration, exercise count, and description.
    /// In DEBUG mode, saves the workout data to a JSON file for debugging.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the selected <see cref="GetWorkoutResponseModel"/> with hybrid V1/V2 data,
    /// or <c>null</c> if no workouts are found.
    /// </returns>
    private static async Task<GetWorkoutResponseModel?> SelectHevyWorkoutAsync()
    {
        var workouts = await _hevyService!.GetWorkoutsAsync();
        
        if (!workouts.Any())
        {
            Console.WriteLine("❌ No Hevy workouts found");
            return null;
        }

        Console.WriteLine($"Found {workouts.Count} Hevy workouts:");
        Console.WriteLine();

        for (int i = 0; i < workouts.Count; i++)
        {
            var workout = workouts[i];
            var startTime = workout.StartTime; //DateTimeOffset.FromUnixTimeSeconds(workout.StartTime);
            var endTime = workout.EndTime; //DateTimeOffset.FromUnixTimeSeconds(workout.EndTime);
            var duration = endTime - startTime;

            Console.WriteLine($"{i + 1,2}. {workout.Title}");
            Console.WriteLine($"     Date: {startTime:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"     Duration: {duration:hh\\:mm\\:ss}");
            Console.WriteLine($"     Exercises: {workout.Exercises.Count}");
            if (!string.IsNullOrEmpty(workout.Description))
                Console.WriteLine($"     Description: {workout.Description}");
            Console.WriteLine();
        }

        while (true)
        {
            Console.Write($"Select a workout (1-{workouts.Count}): ");
            if (int.TryParse(Console.ReadLine(), out int selection) && 
                selection >= 1 && selection <= workouts.Count)
            {
                var selected = workouts[selection - 1];
#if DEBUG
                var fileName = $"hevy_workout_fromlist_{selected.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var content = JsonSerializer.Serialize(selected, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(fileName, content);
#endif
                Console.WriteLine($"✅ Selected: {selected.Title}");

                var hybridWorkout = await _hevyService.GetWorkoutResponseHybrid(selected.Id);

                return hybridWorkout;
            }
            Console.WriteLine("Invalid selection. Please try again.");
        }
    }

    /// <summary>
    /// Synchronizes heart rate data from a Strava activity to a Hevy workout.
    /// Generates synchronized biometrics, displays statistics, saves data to a file,
    /// prompts for confirmation, and optionally updates the Hevy workout.
    /// If the update succeeds, offers to delete the old workout to prevent duplicates.
    /// </summary>
    /// <param name="stravaActivity">The detailed Strava activity containing the source data.</param>
    /// <param name="heartRateStream">The heart rate stream data from Strava.</param>
    /// <param name="hevyWorkout">The target Hevy workout to synchronize with.</param>
    /// <returns>A task representing the asynchronous synchronization operation.</returns>
    private static async Task SynchronizeHeartRateAsync(
        StravaDetailedActivity stravaActivity,
        StravaHeartRateStream heartRateStream,
        GetWorkoutResponseModel hevyWorkout)
    {
        Console.WriteLine($"Synchronizing heart rate data from Strava activity '{stravaActivity.Name}'");
        Console.WriteLine($"to Hevy workout '{hevyWorkout.GetWorkoutResponseV1.Title}'");
        Console.WriteLine();

        // Generate synchronized biometrics
        var biometrics = HeartRateSynchronizerService.SynchronizeHeartRateData(
            stravaActivity, heartRateStream, hevyWorkout);

        Console.WriteLine($"Generated {biometrics.HeartRateSamples.Count} heart rate samples");
        Console.WriteLine($"Total calories: {biometrics.TotalCalories:F0}");

        if (biometrics.HeartRateSamples.Any())
        {
            var avgHr = biometrics.HeartRateSamples.Average(s => s.Bpm);
            var maxHr = biometrics.HeartRateSamples.Max(s => s.Bpm);
            var minHr = biometrics.HeartRateSamples.Min(s => s.Bpm);
            
            Console.WriteLine($"Heart rate - Min: {minHr} bpm, Avg: {avgHr:F0} bpm, Max: {maxHr} bpm");
        }

        // Save synchronized data for debugging
        var fileName = $"synchronized_biometrics_{hevyWorkout.GetWorkoutResponseV1.Id}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        var json = JsonSerializer.Serialize(biometrics, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(fileName, json);
        Console.WriteLine($"Synchronized data saved to: {fileName}");

        // Ask for confirmation before updating Hevy
        Console.WriteLine();
        Console.Write("Do you want to update the Hevy workout with this heart rate data? (y/N): ");
        var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();

        if (confirm == "y" || confirm == "yes")
        {
            Console.WriteLine("Updating Hevy workout...");
            var success = await _hevyService!.UpdateWorkoutBiometricsAsync(hevyWorkout, biometrics, hevyWorkout.GetWorkoutResponseV1.Title, hevyWorkout.GetWorkoutResponseV1.StartTime, hevyWorkout.GetWorkoutResponseV1.EndTime);

            if (success)
            {
                Console.WriteLine("✅ Hevy workout updated successfully!");
                Console.WriteLine();
                Console.WriteLine("⚠️  IMPORTANT: A new workout with heart rate data has been created in Hevy.");
                Console.WriteLine($"   Old workout ID: {hevyWorkout.GetWorkoutResponseV1.Id}");
                Console.WriteLine();
                Console.WriteLine("📱 Please check the Hevy app to verify the new workout appears correctly");
                Console.WriteLine("   before deleting the old one to avoid duplicates.");
                Console.WriteLine();
                Console.Write("Have you verified the new workout in Hevy and want to delete the old one? (y/N): ");
                var deleteConfirm = Console.ReadLine()?.Trim().ToLowerInvariant();

                if (deleteConfirm == "y" || deleteConfirm == "yes")
                {
                    try
                    {
                        Console.WriteLine($"Deleting old workout (ID: {hevyWorkout.GetWorkoutResponseV1.Id})...");
                        var deletedOld = await _hevyService.DeleteWorkoutV2Async(hevyWorkout.GetWorkoutResponseV1.Id);
                        
                        if (deletedOld != null)
                        {
                            Console.WriteLine("✅ Old workout deleted successfully!");
                        }
                        else
                        {
                            Console.WriteLine("⚠️  Warning: Failed to delete old Hevy workout.");
                            Console.WriteLine($"   You may need to manually delete workout ID: {hevyWorkout.GetWorkoutResponseV1.Id}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️  Warning: Failed to delete old Hevy workout: {ex.Message}");
                        Console.WriteLine($"   You may need to manually delete workout ID: {hevyWorkout.GetWorkoutResponseV1.Id}");
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("⚠️  Reminder: You should delete the old workout manually to avoid duplicates.");
                    Console.WriteLine($"   Old workout ID: {hevyWorkout.GetWorkoutResponseV1.Id}");
                    Console.WriteLine($"   Workout Title: {hevyWorkout.GetWorkoutResponseV1.Title}");
                    Console.WriteLine($"   Date: {hevyWorkout.GetWorkoutResponseV1.StartTime:yyyy-MM-dd HH:mm}");
                }
            }
            else
            {
                Console.WriteLine("❌ Failed to update Hevy workout. Check your API key and permissions.");
                Console.WriteLine("💡 Note: The v2 API endpoint may not be available yet. Check the synchronized data file for manual upload.");
            }
        }
        else
        {
            Console.WriteLine("Skipped updating Hevy workout.");
        }
    }
}