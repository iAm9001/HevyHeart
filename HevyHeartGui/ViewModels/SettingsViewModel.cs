using System.Windows.Input;
using HevyHeartConsole.Config;
using HevyHeartGui.Commands;

namespace HevyHeartGui.ViewModels;

/// <summary>
/// ViewModel for the Settings Dialog window.
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    private readonly AppConfig _config;
    private string _stravaClientId;
    private string _stravaClientSecret;
    private string _stravaRedirectUri;
    private string _hevyApiKey;
    private string _hevyAccessToken;
    private string _hevyRefreshToken;

    public SettingsViewModel(AppConfig config)
    {
        _config = config;

        // Load current values
        _stravaClientId = config.Strava.ClientId ?? string.Empty;
        _stravaClientSecret = config.Strava.ClientSecret ?? string.Empty;
        _stravaRedirectUri = config.Strava.RedirectUri ?? "http://localhost:8080/callback";
        _hevyApiKey = config.Hevy.ApiKey ?? string.Empty;
        _hevyAccessToken = config.Hevy.AccessToken ?? string.Empty;
        _hevyRefreshToken = config.Hevy.RefreshToken ?? string.Empty;

        SaveCommand = new RelayCommand(_ => SaveSettings(), _ => CanSave());
        CancelCommand = new RelayCommand(_ => { DialogResult = false; });
    }

    #region Properties

    public string StravaClientId
    {
        get => _stravaClientId;
        set
        {
            if (SetProperty(ref _stravaClientId, value))
            {
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string StravaClientSecret
    {
        get => _stravaClientSecret;
        set
        {
            if (SetProperty(ref _stravaClientSecret, value))
            {
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string StravaRedirectUri
    {
        get => _stravaRedirectUri;
        set
        {
            if (SetProperty(ref _stravaRedirectUri, value))
            {
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string HevyApiKey
    {
        get => _hevyApiKey;
        set
        {
            if (SetProperty(ref _hevyApiKey, value))
            {
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string HevyAccessToken
    {
        get => _hevyAccessToken;
        set
        {
            if (SetProperty(ref _hevyAccessToken, value))
            {
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string HevyRefreshToken
    {
        get => _hevyRefreshToken;
        set
        {
            if (SetProperty(ref _hevyRefreshToken, value))
            {
                ((RelayCommand)SaveCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public bool? DialogResult { get; set; }

    #endregion

    #region Commands

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    #endregion

    #region Methods

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(StravaClientId) &&
               !string.IsNullOrWhiteSpace(StravaClientSecret) &&
               !string.IsNullOrWhiteSpace(StravaRedirectUri) &&
               !string.IsNullOrWhiteSpace(HevyApiKey);
        // Hevy username and password are optional
    }

    public void SaveSettings()
    {
        // Update config object
        _config.Strava.ClientId = StravaClientId.Trim();
        _config.Strava.ClientSecret = StravaClientSecret.Trim();
        _config.Strava.RedirectUri = StravaRedirectUri.Trim();
        _config.Hevy.ApiKey = HevyApiKey.Trim();
        
        // Only save tokens if they were provided
        if (!string.IsNullOrWhiteSpace(HevyAccessToken))
            _config.Hevy.AccessToken = HevyAccessToken.Trim();
        
        if (!string.IsNullOrWhiteSpace(HevyRefreshToken))
            _config.Hevy.RefreshToken = HevyRefreshToken.Trim();

        DialogResult = true;
    }

    #endregion
}
