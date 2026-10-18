using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using HevyHeartConsole.Config;
using HevyHeartConsole.Services;
using HevyHeartGui.Commands;
using HevyHeartModels.Hevy.V1;
using HevyHeartModels.Internal;
using HevyHeartModels.Strava;
using HevyHeartModels.Enums;
using Microsoft.Win32;

namespace HevyHeartGui.ViewModels;

/// <summary>
/// Main ViewModel for the HevyHeart GUI application.
/// Manages the workflow for synchronizing heart rate data from Strava to Hevy.
/// </summary>
public class MainViewModel : ViewModelBase
{
    private readonly AppConfig _config;
    private StravaService? _stravaService;
    private HevyService? _hevyService;

    // Authentication
    private bool _isStravaAuthenticated;
    private bool _isHevyAuthenticated;
    private string _hevyAccessToken = string.Empty;
    private string _hevyRefreshToken = string.Empty;

    // Activities and Workouts
    private StravaActivity? _selectedStravaActivity;
    private GetWorkoutResponseModel? _selectedHevyWorkout;
    private StravaDetailedActivity? _detailedActivity;
    private StravaHeartRateStream? _heartRateStream;

    // UI State
    private string _statusMessage = "Ready to sync heart rate data";
    private bool _isLoading;
    private string _syncSummary = string.Empty;
    private WatchType _selectedWatchType = WatchType.None;
    private bool _republishToStrava = false;

    public MainViewModel(AppConfig config)
    {
        _config = config;
        
        // Initialize services
        _stravaService = new StravaService(new System.Net.Http.HttpClient(), _config.Strava);
        _hevyService = new HevyService(new System.Net.Http.HttpClient(), _config.Hevy);

        // Initialize collections
        StravaActivities = new ObservableCollection<StravaActivity>();
        HevyWorkouts = new ObservableCollection<HevyWorkout>();

        // Initialize commands
        AuthenticateHevyCommand = new AsyncRelayCommand(async _ => await AuthenticateHevyAsync(), _ => !IsHevyAuthenticated && !IsLoading);
        AuthenticateStravaCommand = new AsyncRelayCommand(async _ => await AuthenticateStravaAsync(), _ => !IsStravaAuthenticated && !IsLoading);
        LoadStravaActivitiesCommand = new AsyncRelayCommand(async _ => await LoadStravaActivitiesAsync(), _ => IsStravaAuthenticated && !IsLoading);
        LoadHevyWorkoutsCommand = new AsyncRelayCommand(async _ => await LoadHevyWorkoutsAsync(), _ => IsHevyAuthenticated && !IsLoading);
        LoadActivityDetailsCommand = new AsyncRelayCommand(async _ => await LoadActivityDetailsAsync(), _ => SelectedStravaActivity != null && !IsLoading);
        SynchronizeCommand = new AsyncRelayCommand(async _ => await SynchronizeHeartRateAsync(), _ => CanSynchronize() && !IsLoading);
        WebLoginHevyCommand = new AsyncRelayCommand(async _ => await WebLoginHevyAsync(), _ => !IsHevyAuthenticated && !IsLoading);
        ImportWorkoutFromJsonCommand = new AsyncRelayCommand(async _ => await ImportWorkoutFromJsonAsync(), _ => !IsLoading);

        // Load OAuth tokens from config if available
        if (!string.IsNullOrEmpty(_config.Hevy.AccessToken))
            HevyAccessToken = _config.Hevy.AccessToken;
        if (!string.IsNullOrEmpty(_config.Hevy.RefreshToken))
            HevyRefreshToken = _config.Hevy.RefreshToken;

        // Auto-authenticate if tokens are configured
        _ = Task.Run(async () =>
        {
            if (!string.IsNullOrEmpty(_config.Hevy.AccessToken) && !string.IsNullOrEmpty(_config.Hevy.RefreshToken))
            {
                await AuthenticateHevyAsync();
            }
        });
    }

    #region Properties

    public ObservableCollection<StravaActivity> StravaActivities { get; }
    public ObservableCollection<HevyWorkout> HevyWorkouts { get; }

    public bool IsStravaAuthenticated
    {
        get => _isStravaAuthenticated;
        set => SetProperty(ref _isStravaAuthenticated, value);
    }

    public bool IsHevyAuthenticated
    {
        get => _isHevyAuthenticated;
        set => SetProperty(ref _isHevyAuthenticated, value);
    }

    public string HevyAccessToken
    {
        get => _hevyAccessToken;
        set => SetProperty(ref _hevyAccessToken, value);
    }

    public string HevyRefreshToken
    {
        get => _hevyRefreshToken;
        set => SetProperty(ref _hevyRefreshToken, value);
    }

    public StravaActivity? SelectedStravaActivity
    {
        get => _selectedStravaActivity;
        set
        {
            if (SetProperty(ref _selectedStravaActivity, value))
            {
                ((AsyncRelayCommand)LoadActivityDetailsCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)SynchronizeCommand).RaiseCanExecuteChanged();

                // Clear any previous synchronization preview when user selects a different activity
                SyncSummary = string.Empty;
            }
        }
    }

    public GetWorkoutResponseModel? SelectedHevyWorkout
    {
        get => _selectedHevyWorkout;
        set
        {
            if (SetProperty(ref _selectedHevyWorkout, value))
            {
                ((AsyncRelayCommand)SynchronizeCommand).RaiseCanExecuteChanged();
            }
        }
    }

    private HevyWorkout? _selectedHevyWorkoutItem;
    public HevyWorkout? SelectedHevyWorkoutItem
    {
        get => _selectedHevyWorkoutItem;
        set
        {
            if (SetProperty(ref _selectedHevyWorkoutItem, value))
            {
                _ = Task.Run(async () =>
                {
                    if (value != null)
                    {
                        await LoadHevyWorkoutDetailsAsync(value.Id);
                    }
                });
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                ((AsyncRelayCommand)AuthenticateHevyCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)AuthenticateStravaCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)LoadStravaActivitiesCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)LoadHevyWorkoutsCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)LoadActivityDetailsCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)SynchronizeCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)WebLoginHevyCommand).RaiseCanExecuteChanged();
                ((AsyncRelayCommand)ImportWorkoutFromJsonCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string SyncSummary
    {
        get => _syncSummary;
        set => SetProperty(ref _syncSummary, value);
    }

    public WatchType SelectedWatchType
    {
        get => _selectedWatchType;
        set => SetProperty(ref _selectedWatchType, value);
    }

    public bool RepublishToStrava
    {
        get => _republishToStrava;
        set => SetProperty(ref _republishToStrava, value);
    }

    #endregion

    #region Commands

    public ICommand AuthenticateHevyCommand { get; }
    public ICommand AuthenticateStravaCommand { get; }
    public ICommand LoadStravaActivitiesCommand { get; }
    public ICommand LoadHevyWorkoutsCommand { get; }
    public ICommand LoadActivityDetailsCommand { get; }
    public ICommand SynchronizeCommand { get; }
    public ICommand WebLoginHevyCommand { get; }
    public ICommand ImportWorkoutFromJsonCommand { get; }

    #endregion

    #region Methods

    private async Task AuthenticateHevyAsync()
    {
        IsLoading = true;
        StatusMessage = "Authenticating with Hevy...";

        try
        {
            if (string.IsNullOrEmpty(HevyAccessToken) || string.IsNullOrEmpty(HevyRefreshToken))
            {
                StatusMessage = "Please enter Hevy OAuth tokens";
                MessageBox.Show(
                    "Please enter your Hevy access_token and refresh_token.\n\n" +
                    "Obtain these from the \"auth2.0-token\" cookie after logging in at https://app.hevyapp.com:\n" +
                    "1. Log in at https://app.hevyapp.com\n" +
                    "2. Open DevTools (F12) > Application > Cookies > hevy.com\n" +
                    "3. Find \"auth2.0-token\", URL-decode and parse the JSON value\n" +
                    "4. Copy \"access_token\" and \"refresh_token\"",
                    "Authentication Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var success = await _hevyService!.LoginAsync(HevyAccessToken, HevyRefreshToken);
            
            if (success)
            {
                IsHevyAuthenticated = true;
                StatusMessage = "Successfully authenticated with Hevy";
                await LoadHevyWorkoutsAsync();
            }
            else
            {
                StatusMessage = "Hevy authentication failed";
                MessageBox.Show(
                    "Failed to authenticate with Hevy.\n\n" +
                    "Things to check:\n" +
                    "• Make sure you copied the full token value (they are very long JWT strings starting with 'eyJ')\n" +
                    "• Ensure there are no leading/trailing spaces\n" +
                    "• The token may have expired — re-obtain it from the browser cookie\n\n" +
                    "In your browser console on app.hevyapp.com, run:\n" +
                    "  JSON.parse(decodeURIComponent(document.cookie.match(/auth2\\.0-token=([^;]+)/)[1]))",
                    "Authentication Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"Error authenticating with Hevy: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task WebLoginHevyAsync()
    {
        try
        {
            // Clear existing authentication state to force a fresh login
            _hevyService?.Logout();
            IsHevyAuthenticated = false;
            HevyAccessToken = string.Empty;
            HevyRefreshToken = string.Empty;
            
            // Clear any loaded workouts
            HevyWorkouts.Clear();
            SelectedHevyWorkout = null;
            SelectedHevyWorkoutItem = null;
            
            var loginWindow = new HevyWebLoginWindow
            {
                Owner = Application.Current.MainWindow
            };
            StatusMessage = "Waiting for Hevy login...";
            var result = loginWindow.ShowDialog();
            if (result == true && loginWindow.AccessToken != null)
            {
                HevyAccessToken = loginWindow.AccessToken;
                HevyRefreshToken = loginWindow.RefreshToken ?? string.Empty;

                // Persist captured tokens to config
                _config.Hevy.AccessToken = HevyAccessToken;
                _config.Hevy.RefreshToken = HevyRefreshToken;

                await AuthenticateHevyAsync();
            }
            else
            {
                StatusMessage = "Login cancelled";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"Error during web login: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task AuthenticateStravaAsync()
    {
        try
        {
            var authUrl = _stravaService!.GetAuthorizationUrl();
            var loginWindow = new StravaWebLoginWindow(authUrl, _config.Strava.RedirectUri)
            {
                Owner = Application.Current.MainWindow
            };
            
            StatusMessage = "Waiting for Strava authorization...";
            var result = loginWindow.ShowDialog();
            
            if (result == true && loginWindow.AuthorizationCode != null)
            {
                IsLoading = true;
                StatusMessage = "Exchanging authorization code for access token...";

                var success = await _stravaService.ExchangeCodeForTokenAsync(loginWindow.AuthorizationCode);

                if (success)
                {
                    IsStravaAuthenticated = true;
                    StatusMessage = "Successfully authenticated with Strava";
                    await LoadStravaActivitiesAsync();
                }
                else
                {
                    StatusMessage = "Strava authentication failed";
                    MessageBox.Show("Failed to exchange authorization code for access token.", "Authentication Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                StatusMessage = "Strava authorization cancelled";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"Error authenticating with Strava: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadStravaActivitiesAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading Strava activities...";

        try
        {
            var activities = await _stravaService!.GetActivitiesAsync();
            
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                StravaActivities.Clear();
                foreach (var activity in activities)
                {
                    StravaActivities.Add(activity);
                }
            });

            StatusMessage = $"Loaded {activities.Count} Strava activities with heart rate data";
        }
        catch (Exception ex)
        {
            StatusMessage = $"? Error: {ex.Message}";
            MessageBox.Show($"Error loading Strava activities: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadHevyWorkoutsAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading Hevy workouts...";

        try
        {
            var workouts = await _hevyService!.GetWorkoutsAsync();
            
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                HevyWorkouts.Clear();
                foreach (var workout in workouts)
                {
                    HevyWorkouts.Add(workout);
                }
            });

            StatusMessage = $"Loaded {workouts.Count} Hevy workouts";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"Error loading Hevy workouts: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadActivityDetailsAsync()
    {
        if (SelectedStravaActivity == null) return;

        IsLoading = true;
        StatusMessage = "Loading activity details...";

        try
        {
            _detailedActivity = await _stravaService!.GetActivityAsync(SelectedStravaActivity.Id);
            _heartRateStream = await _stravaService.GetHeartRateStreamAsync(SelectedStravaActivity.Id);

            if (_detailedActivity != null && _heartRateStream != null)
            {
                // Indicate that the synchronization preview is available immediately
                StatusMessage = $"Synchronization preview ready for '{SelectedStravaActivity.Name}'";
                ((AsyncRelayCommand)SynchronizeCommand).RaiseCanExecuteChanged();

                // Immediately populate the synchronization preview with Strava activity details
                try
                {
                    var start = SelectedStravaActivity.StartDate;
                    // ElapsedTime is provided as total seconds in Strava models. Convert to TimeSpan
                    var durationSeconds = SelectedStravaActivity.ElapsedTime;
                    var ts = TimeSpan.FromSeconds(durationSeconds);
                    // Use total hours so durations >24h render correctly (e.g., 27:15:30)
                    var totalHours = (int)ts.TotalHours;
                    var durationStr = string.Format("{0:D2}:{1:D2}:{2:D2}", totalHours, ts.Minutes, ts.Seconds);

                    string avgHrText = "N/A";
                    if (SelectedStravaActivity.AverageHeartrate != null)
                    {
                        avgHrText = string.Format("{0:F0}", SelectedStravaActivity.AverageHeartrate);
                    }

                    // Make the preview explicit and compact for smaller displays
                    SyncSummary = "Synchronization Preview:\n" +
                                  SelectedStravaActivity.Name + "\n" +
                                  $"Date: {start:yyyy-MM-dd HH:mm} | Type: {SelectedStravaActivity.Type}\n" +
                                  $"Duration: {durationStr} | Avg HR: {avgHrText} bpm";
                }
                catch
                {
                    // If building the preview fails for any reason, ensure SyncSummary is not left null
                    SyncSummary = string.Empty;
                }
            }
            else
            {
                StatusMessage = "? Failed to load activity details";
                SyncSummary = string.Empty; // clear stale preview on failure
                MessageBox.Show("Failed to load activity details or heart rate stream.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"? Error: {ex.Message}";
            SyncSummary = string.Empty;
            MessageBox.Show($"Error loading activity details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadHevyWorkoutDetailsAsync(string workoutId)
    {
        IsLoading = true;
        StatusMessage = "Loading workout details...";

        try
        {
            SelectedHevyWorkout = await _hevyService!.GetWorkoutResponseHybrid(workoutId);
            
            if (SelectedHevyWorkout != null)
            {
                StatusMessage = $"Loaded details for '{SelectedHevyWorkout.GetWorkoutResponseV1.Title}'";
                ((AsyncRelayCommand)SynchronizeCommand).RaiseCanExecuteChanged();
            }
            else
            {
                StatusMessage = "Failed to load workout details";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"? Error: {ex.Message}";
            MessageBox.Show($"Error loading workout details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SynchronizeHeartRateAsync()
    {
        if (SelectedStravaActivity == null || SelectedHevyWorkout == null || _detailedActivity == null || _heartRateStream == null)
            return;

        IsLoading = true;
        StatusMessage = "Synchronizing heart rate data...";

        try
        {
            // Generate synchronized biometrics
            var biometrics = HeartRateSynchronizerService.SynchronizeHeartRateData(
                _detailedActivity, _heartRateStream, SelectedHevyWorkout);

            var avgHr = biometrics.HeartRateSamples.Any() ? biometrics.HeartRateSamples.Average(s => s.Bpm) : 0;
            var maxHr = biometrics.HeartRateSamples.Any() ? biometrics.HeartRateSamples.Max(s => s.Bpm) : 0;
            var minHr = biometrics.HeartRateSamples.Any() ? biometrics.HeartRateSamples.Min(s => s.Bpm) : 0;

            SyncSummary = $"Samples: {biometrics.HeartRateSamples.Count}\n" +
                         $"Calories: {biometrics.TotalCalories:F0}\n" +
                         $"HR Min: {minHr} bpm\n" +
                         $"HR Avg: {avgHr:F0} bpm\n" +
                         $"HR Max: {maxHr} bpm";

            var republishLine = RepublishToStrava
                ? "📤 The Hevy-published Strava activity (the one Hevy created automatically when you finished the workout) " +
                  "will be found, deleted, and replaced with a new upload containing the synchronized heart rate data.\n" +
                  "   Note: requires 'activity:write' Strava scope.\n\n"
                : "🚫 The Hevy-published Strava activity will be left unchanged.\n\n";

            // Ask for confirmation
            var result = MessageBox.Show(
                $"Ready to sync heart rate data:\n\n{SyncSummary}\n\n" +
                $"From Strava: {SelectedStravaActivity.Name}\n" +
                $"To Hevy:     {SelectedHevyWorkout.GetWorkoutResponseV1.Title}\n\n" +
                republishLine +
                "⚠️  The original Hevy workout will be deleted and recreated with the same ID.\n" +
                "Do you want to proceed?",
                "Confirm Synchronization",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                StatusMessage = "Synchronization cancelled";
                return;
            }

            // Delete the old Hevy workout and recreate with heart rate data (Hevy delete is inside the service).
            StatusMessage = "Deleting and recreating Hevy workout with heart rate data...";
            var success = await _hevyService!.UpdateWorkoutBiometricsAsync(
                SelectedHevyWorkout,
                biometrics,
                SelectedHevyWorkout.GetWorkoutResponseV1.Title,
                SelectedHevyWorkout.GetWorkoutResponseV1.StartTime,
                SelectedHevyWorkout.GetWorkoutResponseV1.EndTime,
                SelectedWatchType,
                shareToStrava: false);

            if (success)
            {
                StatusMessage = "✅ Hevy workout recreated successfully with heart rate data!";
                MessageBox.Show(
                    "✅ Hevy workout recreated successfully with heart rate data!",
                    "Hevy Update Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                if (RepublishToStrava)
                {
                    StatusMessage = "🔍 Searching for Hevy-published Strava activity...";

                    var candidates = await _stravaService!.FindActivitiesNearTimeAsync(
                        SelectedHevyWorkout.GetWorkoutResponseV1.StartTime,
                        SelectedStravaActivity.Id);

                    if (!candidates.Any())
                    {
                        MessageBox.Show(
                            "No Strava activity was found near the Hevy workout start time (±30 minutes).\n\n" +
                            "The Hevy-published activity may not exist yet or may fall outside the search window.\n" +
                            "You may need to update the Strava activity manually.",
                            "Activity Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        // Present candidate(s) to the user for confirmation
                        var candidateLines = string.Join("\n", candidates.Select((a, i) =>
                            $"{i + 1}. {a.Name}  ({a.StartDate:yyyy-MM-dd HH:mm})  HR: {(a.HasHeartrate ? "Yes" : "No")}  Ext ID: {a.ExternalId ?? "(none)"}"));

                        StravaActivity? targetActivity = null;
                        if (candidates.Count == 1)
                        {
                            var confirmResult = MessageBox.Show(
                                $"Found candidate Strava activity:\n\n{candidateLines}\n\n" +
                                "Is this the Hevy-published activity?\n\n" +
                                "Selecting YES will delete this activity and upload a new one with the synchronized heart rate data.",
                                "Confirm Strava Activity Update",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);
                            if (confirmResult == MessageBoxResult.Yes)
                                targetActivity = candidates[0];
                        }
                        else
                        {
                            // Multiple candidates — ask the user to pick the most likely one (prefer no-HR activities)
                            var bestGuess = candidates.FirstOrDefault(a => !a.HasHeartrate) ?? candidates[0];
                            var confirmResult = MessageBox.Show(
                                $"Found {candidates.Count} candidate Strava activities near the workout time:\n\n{candidateLines}\n\n" +
                                $"Best guess (no HR data): {bestGuess.Name} ({bestGuess.StartDate:yyyy-MM-dd HH:mm})\n\n" +
                                "Update this activity with the synchronized heart rate data?\n" +
                                "(If this is not the right activity, click No and update manually.)",
                                "Confirm Strava Activity Update",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);
                            if (confirmResult == MessageBoxResult.Yes)
                                targetActivity = bestGuess;
                        }

                        if (targetActivity != null)
                        {
                            var deleteResult = MessageBox.Show(
                                $"To update '{targetActivity.Name}' with heart rate data, delete the current Strava activity first.\n\n" +
                                "Continue to the in-app Strava delete window?",
                                "Delete Strava Activity",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Question);

                            if (deleteResult == MessageBoxResult.Yes)
                            {
                                StatusMessage = "Opening in-app Strava delete window...";
                                var webDeleteResult = await StravaWebLoginWindow.DeleteActivityViaWebSessionAsync(
                                    Application.Current.MainWindow,
                                    targetActivity.Id,
                                    targetActivity.Name);

                                if (!webDeleteResult.Success)
                                {
                                    StatusMessage = "Strava delete step was not completed";
                                    MessageBox.Show(
                                        $"Could not complete the in-app delete step (status: {webDeleteResult.StatusCode}).\n\n" +
                                        "Please retry and delete the activity in the embedded Strava window before uploading.",
                                        "Delete Step Failed",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                                    return;
                                }

                                StatusMessage = "📤 Uploading new Strava activity with heart rate data...";

                                var hevyV1 = SelectedHevyWorkout.GetWorkoutResponseV1;
                                var durationSeconds = (int)(hevyV1.EndTime - hevyV1.StartTime).TotalSeconds;

                                var uploadResult = await _stravaService.UploadActivityWithHeartRateAsync(
                                    title: hevyV1.Title,
                                    activityType: "WeightTraining",
                                    startTime: hevyV1.StartTime,
                                    durationSeconds: durationSeconds,
                                    biometrics: biometrics,
                                    externalId: $"hevyheart-{hevyV1.Id}");

                                if (uploadResult != null && string.IsNullOrEmpty(uploadResult.Error))
                                {
                                    StatusMessage = "✅ Strava activity updated with heart rate data!";
                                    MessageBox.Show(
                                        $"✅ Strava activity updated with heart rate data!\n\n" +
                                        $"Upload job ID: {uploadResult.Id}\n" +
                                        $"Status: {uploadResult.Status}\n\n" +
                                        "Strava will process the activity shortly and it will appear in your feed.",
                                        "Strava Update Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                else
                                {
                                    StatusMessage = "❌ Strava upload failed";
                                    MessageBox.Show(
                                        $"Failed to upload the activity to Strava:\n{uploadResult?.Error ?? "Unknown error"}",
                                        "Upload Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                }

                // Reload workouts to reflect the recreated entry
                await LoadHevyWorkoutsAsync();
            }
            else
            {
                StatusMessage = "❌ Failed to synchronize heart rate data";
                MessageBox.Show(
                    "Failed to recreate Hevy workout. Check your API key and permissions.\n\n" +
                    "⚠️  The original Hevy workout has already been deleted by this point.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"❌ Error: {ex.Message}";
            MessageBox.Show($"Error synchronizing heart rate data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ImportWorkoutFromJsonAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Import Hevy Workout from JSON",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = ".json"
        };

        if (dialog.ShowDialog() != true) return;

        IsLoading = true;
        StatusMessage = $"Importing workout from {Path.GetFileName(dialog.FileName)}...";

        try
        {
            var json = await File.ReadAllTextAsync(dialog.FileName);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Try V1 GetWorkoutResponse first (superset of HevyWorkout fields)
            HevyHeartModels.Hevy.V1.GetWorkoutResponse? v1Response = null;
            try
            {
                v1Response = JsonSerializer.Deserialize<HevyHeartModels.Hevy.V1.GetWorkoutResponse>(json, options);
                if (string.IsNullOrEmpty(v1Response?.Id))
                    v1Response = null;
            }
            catch { v1Response = null; }

            // Fallback: try HevyWorkout (list-item format) and convert to V1 response shape
            if (v1Response == null)
            {
                HevyWorkout? listWorkout = null;
                try
                {
                    listWorkout = JsonSerializer.Deserialize<HevyWorkout>(json, options);
                    if (string.IsNullOrEmpty(listWorkout?.Id))
                        listWorkout = null;
                }
                catch { listWorkout = null; }

                if (listWorkout != null)
                {
                    v1Response = new HevyHeartModels.Hevy.V1.GetWorkoutResponse
                    {
                        Id = listWorkout.Id,
                        Title = listWorkout.Title,
                        Description = listWorkout.Description,
                        RoutineId = listWorkout.RoutineId,
                        StartTime = listWorkout.StartTime,
                        EndTime = listWorkout.EndTime,
                        Exercises = listWorkout.Exercises
                            .Select((e, idx) => new HevyHeartModels.Hevy.V1.V1Exercise
                            {
                                Index = idx,
                                Title = e.Title,
                                ExerciseTemplateId = e.ExerciseTemplateId,
                                Notes = string.Empty,
                                Sets = e.Sets.Select(s => new HevyHeartModels.Hevy.V1.V1Set
                                {
                                    Index = s.Index,
                                    Type = s.Type,
                                    WeightKg = s.WeightKg,
                                    Reps = s.Reps,
                                    DistanceMeters = s.DistanceMeters,
                                    DurationSeconds = s.DurationSeconds
                                }).ToList()
                            }).ToList()
                    };
                }
            }

            if (v1Response == null)
            {
                MessageBox.Show(
                    "Could not parse the selected file as a Hevy workout.\n\n" +
                    "Expected a V1 GetWorkoutResponse or HevyWorkout JSON file.\n" +
                    "These are saved automatically in DEBUG mode, e.g.:\n" +
                    "  hevy_workout_<id>_<timestamp>.json\n" +
                    "  hevy_workout_fromlist_<id>_<timestamp>.json",
                    "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Build V2 response: fetch from API if authenticated, otherwise synthesize from V1
            HevyHeartModels.Hevy.V2.GetWorkoutResponse v2Response;
            if (IsHevyAuthenticated && _hevyService != null)
            {
                try
                {
                    StatusMessage = "Fetching V2 workout data from Hevy API...";
                    v2Response = await _hevyService.GetWorkoutV2Async(v1Response.Id);
                }
                catch
                {
                    // Workout may no longer exist on server (deleted) — build minimal V2 from V1 data
                    v2Response = BuildMinimalV2Response(v1Response);
                }
            }
            else
            {
                v2Response = BuildMinimalV2Response(v1Response);
            }

            var responseModel = new GetWorkoutResponseModel(v1Response, v2Response);

            // Build a HevyWorkout entry for display in the list
            var displayWorkout = new HevyWorkout
            {
                Id = v1Response.Id,
                Title = v1Response.Title,
                Description = v1Response.Description,
                RoutineId = v1Response.RoutineId,
                StartTime = v1Response.StartTime,
                EndTime = v1Response.EndTime,
                Exercises = v1Response.Exercises.Select(e => new HevyExercise
                {
                    Title = e.Title,
                    ExerciseTemplateId = e.ExerciseTemplateId,
                    Sets = e.Sets.Select(s => new HevySet
                    {
                        Index = s.Index,
                        Type = s.Type,
                        WeightKg = s.WeightKg,
                        Reps = s.Reps,
                        DistanceMeters = s.DistanceMeters,
                        DurationSeconds = s.DurationSeconds
                    }).ToList()
                }).ToList()
            };

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // Insert at the top of the workouts list
                HevyWorkouts.Insert(0, displayWorkout);

                // Select the imported workout and set the full hybrid model directly,
                // bypassing the API-fetching SelectedHevyWorkoutItem setter.
                SelectedHevyWorkout = responseModel;
                _selectedHevyWorkoutItem = displayWorkout;
                OnPropertyChanged(nameof(SelectedHevyWorkoutItem));
                ((AsyncRelayCommand)SynchronizeCommand).RaiseCanExecuteChanged();
            });

            StatusMessage = $"✅ Imported '{v1Response.Title}' from {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show($"Error importing workout: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Builds a minimal V2 <see cref="HevyHeartModels.Hevy.V2.GetWorkoutResponse"/> from V1 data.
    /// Used when the workout is no longer available on the Hevy server or the user is not authenticated.
    /// Rest timer and volume-doubling defaults to 0 / false; set completion timestamps are left empty.
    /// </summary>
    private static HevyHeartModels.Hevy.V2.GetWorkoutResponse BuildMinimalV2Response(
        HevyHeartModels.Hevy.V1.GetWorkoutResponse v1)
    {
        return new HevyHeartModels.Hevy.V2.GetWorkoutResponse
        {
            Id = v1.Id,
            Name = v1.Title,
            Description = v1.Description,
            RoutineId = v1.RoutineId,
            TrainerProgramId = v1.TrainerProgramId,
            StartTime = ((DateTimeOffset)v1.StartTime).ToUnixTimeSeconds(),
            EndTime = ((DateTimeOffset)v1.EndTime).ToUnixTimeSeconds(),
            Exercises = v1.Exercises.Select(e => new HevyHeartModels.Hevy.V2.GetExercise
            {
                ExerciseTemplateId = e.ExerciseTemplateId,
                Title = e.Title,
                RestSeconds = 0,
                VolumeDoublingEnabled = false,
                Sets = e.Sets.Select(s => new HevyHeartModels.Hevy.V2.GetSet
                {
                    Index = s.Index,
                    CompletedAt = string.Empty,
                    WeightKg = s.WeightKg,
                    Reps = s.Reps,
                    DurationSeconds = s.DurationSeconds,
                    DistanceMeters = s.DistanceMeters,
                    Rpe = s.Rpe
                }).ToList()
            }).ToList()
        };
    }

    private bool CanSynchronize()    {
        return SelectedStravaActivity != null &&
               SelectedHevyWorkout != null &&
               _detailedActivity != null &&
               _heartRateStream != null &&
               IsStravaAuthenticated &&
               IsHevyAuthenticated;
    }

    #endregion
}
