using System.Windows;
using HevyHeartGui.ViewModels;

namespace HevyHeartGui;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;

    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        // Set initial password box values if they exist
        if (!string.IsNullOrEmpty(_viewModel.StravaClientSecret))
            StravaClientSecretBox.Password = _viewModel.StravaClientSecret;

        if (!string.IsNullOrEmpty(_viewModel.HevyApiKey))
            HevyApiKeyBox.Password = _viewModel.HevyApiKey;

        if (!string.IsNullOrEmpty(_viewModel.HevyPassword))
            HevyPasswordBox.Password = _viewModel.HevyPassword;

        // Subscribe to property changes to enable/disable Save button
        _viewModel.PropertyChanged += (s, e) => UpdateSaveButtonState();
        UpdateSaveButtonState();
    }

    private void UpdateSaveButtonState()
    {
        SaveButton.IsEnabled = !string.IsNullOrWhiteSpace(_viewModel.StravaClientId) &&
                               !string.IsNullOrWhiteSpace(_viewModel.StravaClientSecret) &&
                               !string.IsNullOrWhiteSpace(_viewModel.StravaRedirectUri) &&
                               !string.IsNullOrWhiteSpace(_viewModel.HevyApiKey);
        // Hevy username and password are now optional
    }

    private void StravaClientSecretBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.StravaClientSecret = StravaClientSecretBox.Password;
        UpdateSaveButtonState();
    }

    private void HevyApiKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.HevyApiKey = HevyApiKeyBox.Password;
        UpdateSaveButtonState();
    }

    private void HevyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _viewModel.HevyPassword = HevyPasswordBox.Password;
        UpdateSaveButtonState();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Manually call the save logic from ViewModel
        _viewModel.SaveSettings();
        
        // Set dialog result to true to indicate success
        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
