using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using HevyHeartConsole.Config;
using HevyHeartConsole.Infrastructure;
using HevyHeartConsole.Services;
using HevyHeartGui.Commands;
using HevyHeartModels.Hevy.V1;
using HevyHeartModels.Internal;
using HevyHeartModels.Strava;

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
    private string _hevyEmailOrUsername = string.Empty;
    private string _hevyPassword = string.Empty;

    // Activities and Workouts
    private StravaActivity? _selectedStravaActivity;
    private GetWorkoutResponseModel? _selectedHevyWorkout;
    private StravaDetailedActivity? _detailedActivity;
    private StravaHeartRateStream? _heartRateStream;

    // UI State
    private string _statusMessage = "Ready to sync heart rate data";
    private bool _isLoading;
    private string _syncSummary = string.Empty;

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

        // Load credentials from config if available
        if (!string.IsNullOrEmpty(_config.Hevy.EmailOrUsername))
            HevyEmailOrUsername = _config.Hevy.EmailOrUsername;
        if (!string.IsNullOrEmpty(_config.Hevy.Password))
            HevyPassword = _config.Hevy.Password;

        // Auto-authenticate if credentials are configured
        _ = Task.Run(async () =>
        {
            if (!string.IsNullOrEmpty(_config.Hevy.EmailOrUsername) && !string.IsNullOrEmpty(_config.Hevy.Password))
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

    public string HevyEmailOrUsername
    {
        get => _hevyEmailOrUsername;
        set => SetProperty(ref _hevyEmailOrUsername, value);
    }

    public string HevyPassword
    {
        get => _hevyPassword;
        set => SetProperty(ref _hevyPassword, value);
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
            }
        }
    }

    public string SyncSummary
    {
        get => _syncSummary;
        set => SetProperty(ref _syncSummary, value);
    }

    #endregion

    #region Commands

    public ICommand AuthenticateHevyCommand { get; }
    public ICommand AuthenticateStravaCommand { get; }
    public ICommand LoadStravaActivitiesCommand { get; }
    public ICommand LoadHevyWorkoutsCommand { get; }
    public ICommand LoadActivityDetailsCommand { get; }
    public ICommand SynchronizeCommand { get; }

    #endregion

    #region Methods

    private async Task AuthenticateHevyAsync()
    {
        IsLoading = true;
        StatusMessage = "Authenticating with Hevy...";

        try
        {
            if (string.IsNullOrEmpty(HevyEmailOrUsername) || string.IsNullOrEmpty(HevyPassword))
            {
                StatusMessage = "Please enter Hevy credentials";
                MessageBox.Show("Please enter your Hevy email/username and password.", "Authentication Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var success = await _hevyService!.LoginAsync(HevyEmailOrUsername, HevyPassword);
            
            if (success)
            {
                IsHevyAuthenticated = true;
                StatusMessage = "? Successfully authenticated with Hevy";
                await LoadHevyWorkoutsAsync();
            }
            else
            {
                StatusMessage = "? Hevy authentication failed";
                MessageBox.Show("Failed to authenticate with Hevy. Please check your credentials.", "Authentication Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"? Error: {ex.Message}";
            MessageBox.Show($"Error authenticating with Hevy: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task AuthenticateStravaAsync()
    {
        IsLoading = true;
        StatusMessage = "Authenticating with Strava...";

        try
        {
            var authUrl = _stravaService!.GetAuthorizationUrl();
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
                MessageBox.Show($"Please manually open this URL in your browser:\n\n{authUrl}", "Manual Authentication Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            // Wait for callback
            try
            {
                var code = await callbackTask;
                var success = await _stravaService.ExchangeCodeForTokenAsync(code);

                if (success)
                {
                    IsStravaAuthenticated = true;
                    StatusMessage = "? Successfully authenticated with Strava";
                    await LoadStravaActivitiesAsync();
                }
                else
                {
                    StatusMessage = "? Strava authentication failed";
                    MessageBox.Show("Failed to exchange authorization code for access token.", "Authentication Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                callbackServer.Stop();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"? Error: {ex.Message}";
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

            StatusMessage = $"? Loaded {activities.Count} Strava activities with heart rate data";
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

            StatusMessage = $"? Loaded {workouts.Count} Hevy workouts";
        }
        catch (Exception ex)
        {
            StatusMessage = $"? Error: {ex.Message}";
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
                StatusMessage = $"? Loaded details for '{SelectedStravaActivity.Name}'";
                ((AsyncRelayCommand)SynchronizeCommand).RaiseCanExecuteChanged();
            }
            else
            {
                StatusMessage = "? Failed to load activity details";
                MessageBox.Show("Failed to load activity details or heart rate stream.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"? Error: {ex.Message}";
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
                StatusMessage = $"? Loaded details for '{SelectedHevyWorkout.GetWorkoutResponseV1.Title}'";
                ((AsyncRelayCommand)SynchronizeCommand).RaiseCanExecuteChanged();
            }
            else
            {
                StatusMessage = "? Failed to load workout details";
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

            // Ask for confirmation
            var result = MessageBox.Show(
                $"Ready to sync heart rate data:\n\n{SyncSummary}\n\n" +
                $"From: {SelectedStravaActivity.Name}\n" +
                $"To: {SelectedHevyWorkout.GetWorkoutResponseV1.Title}\n\n" +
                $"This will create a new workout in Hevy with heart rate data.\n" +
                $"Do you want to proceed?",
                "Confirm Synchronization",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var success = await _hevyService!.UpdateWorkoutBiometricsAsync(
                    SelectedHevyWorkout,
                    biometrics,
                    SelectedHevyWorkout.GetWorkoutResponseV1.Title,
                    SelectedHevyWorkout.GetWorkoutResponseV1.StartTime,
                    SelectedHevyWorkout.GetWorkoutResponseV1.EndTime);

                if (success)
                {
                    StatusMessage = "? Heart rate data synchronized successfully!";
                    
                    var deleteResult = MessageBox.Show(
                        $"? New workout created successfully!\n\n" +
                        $"Old workout ID: {SelectedHevyWorkout.GetWorkoutResponseV1.Id}\n\n" +
                        $"Please verify the new workout in Hevy app before deleting the old one.\n\n" +
                        $"Do you want to delete the old workout now?",
                        "Delete Old Workout?",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (deleteResult == MessageBoxResult.Yes)
                    {
                        try
                        {
                            await _hevyService.DeleteWorkoutV2Async(SelectedHevyWorkout.GetWorkoutResponseV1.Id);
                            StatusMessage = "? Old workout deleted successfully!";
                            MessageBox.Show("Old workout deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Warning: Failed to delete old workout: {ex.Message}\n\nPlease delete it manually in the Hevy app.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }

                    // Reload workouts
                    await LoadHevyWorkoutsAsync();
                }
                else
                {
                    StatusMessage = "? Failed to synchronize heart rate data";
                    MessageBox.Show("Failed to update Hevy workout. Check your API key and permissions.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                StatusMessage = "Synchronization cancelled";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"? Error: {ex.Message}";
            MessageBox.Show($"Error synchronizing heart rate data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanSynchronize()
    {
        return SelectedStravaActivity != null &&
               SelectedHevyWorkout != null &&
               _detailedActivity != null &&
               _heartRateStream != null &&
               IsStravaAuthenticated &&
               IsHevyAuthenticated;
    }

    #endregion
}
