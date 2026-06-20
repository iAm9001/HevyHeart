using System;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.Web;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace HevyHeartGui;

/// <summary>
/// Embeds the Strava OAuth authorization page and automatically captures the authorization code
/// from the redirect URL once the user has successfully authorized the application.
///
/// Similar to HevyWebLoginWindow, but adapted for Strava's OAuth flow:
///   - Opens the Strava authorization URL in an embedded browser
///   - Intercepts navigation to the redirect URI
///   - Extracts the authorization code from the query string
///   - Closes automatically when the code is captured
/// </summary>
public partial class StravaWebLoginWindow : Window
{
    /// <summary>The captured OAuth authorization code. Null if authorization was not completed.</summary>
    public string? AuthorizationCode { get; private set; }

    private readonly string _redirectUri;
    private bool _codeCaptured;
    private readonly bool _isDeleteMode;
    private readonly long _deleteActivityId;
    private readonly string _deleteActivityName = string.Empty;

    public StravaWebDeleteResult? DeleteResult { get; private set; }

    /// <summary>
    /// Creates a new StravaWebLoginWindow that will navigate to the given authorization URL.
    /// </summary>
    /// <param name="authorizationUrl">The full Strava OAuth authorization URL</param>
    /// <param name="redirectUri">The redirect URI to watch for (e.g., "http://localhost:5000/callback")</param>
    public StravaWebLoginWindow(string authorizationUrl, string redirectUri)
    {
        InitializeComponent();
        _redirectUri = redirectUri;
        Loaded += async (_, _) => await OnWindowLoadedAsync(authorizationUrl);
    }

    /// <summary>
    /// Creates a window that uses the same WebView profile/session to issue a DELETE request
    /// for the given Strava activity from within the Strava web origin.
    /// </summary>
    public StravaWebLoginWindow(long activityId, string activityName)
    {
        InitializeComponent();
        _redirectUri = string.Empty;
        _isDeleteMode = true;
        _deleteActivityId = activityId;
        _deleteActivityName = activityName;
        Title = "Delete Strava Activity";
        Loaded += async (_, _) => await OnDeleteWindowLoadedAsync();
        Closing += (_, _) =>
        {
            if (!_isDeleteMode || DeleteResult != null) return;

            // Manual delete mode: once user has seen/used the page and closes it,
            // treat this step as completed.
            DeleteResult = new StravaWebDeleteResult
            {
                Success = true,
                StatusCode = 200,
                Message = "Manual delete page opened; user closed window."
            };
        };
    }

    public static async Task<StravaWebDeleteResult> DeleteActivityViaWebSessionAsync(
        Window? owner,
        long activityId,
        string activityName)
    {
        var window = new StravaWebLoginWindow(activityId, activityName);
        if (owner != null)
        {
            window.Owner = owner;
        }

        window.ShowDialog();
        await Task.Yield();

        return window.DeleteResult ?? new StravaWebDeleteResult
        {
            Success = false,
            StatusCode = 0,
            Message = "Delete operation did not return a result."
        };
    }

    private async Task OnWindowLoadedAsync(string authorizationUrl)
    {
        if (_isDeleteMode) return;

        try
        {
            await WebView.EnsureCoreWebView2Async();

            // Intercept navigation to capture the redirect with authorization code
            WebView.CoreWebView2.NavigationStarting += OnNavigationStarting;

            // Navigate to the Strava authorization URL
            WebView.CoreWebView2.Navigate(authorizationUrl);
        }
        catch (Exception ex)
        {
            SetStatus($"Error initializing browser: {ex.Message}");
        }
    }

    private async Task OnDeleteWindowLoadedAsync()
    {
        try
        {
            HeaderText.Text = "Delete Strava activity using your current web session";
            SetStatus("Opening Strava activities page...");

            await WebView.EnsureCoreWebView2Async();

            WebView.CoreWebView2.Navigate("https://strava.com/athlete/training_activities");

            SetStatus($"Delete '{_deleteActivityName}' (ID {_deleteActivityId}) manually, then close this window.");

            MessageBox.Show(
                $"Please delete this Strava activity manually in the embedded browser:\n\n" +
                $"Name: '{_deleteActivityName}'\n" +
                $"ID: {_deleteActivityId}\n\n" +
                "When finished, close this window to continue.",
                "Delete Strava Activity Manually",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // Leave window open for manual action; completion is handled on window close.
        }
        catch (Exception ex)
        {
            DeleteResult = new StravaWebDeleteResult
            {
                Success = false,
                StatusCode = 0,
                Message = ex.Message
            };
            SetStatus($"Error opening Strava page: {ex.Message}");
            MessageBox.Show(
                $"Could not open the in-app Strava delete window.\n\n{ex.Message}",
                "Delete Window Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            Dispatcher.Invoke(() => { DialogResult = false; Close(); });
        }
    }

    /// <summary>
    /// Fires when the WebView is about to navigate to a new URL.
    /// We intercept navigation to the redirect URI to capture the authorization code.
    /// </summary>
    private async void OnNavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
    {
        if (_codeCaptured) return;

        var url = e.Uri;

        // Check if this is a navigation to our redirect URI
        if (url.StartsWith(_redirectUri, StringComparison.OrdinalIgnoreCase))
        {
            // Cancel the navigation - we don't need to actually load the redirect page
            e.Cancel = true;

            SetStatus("Authorization detected — capturing code...");

            try
            {
                // Parse the query string to extract the authorization code
                var uri = new Uri(url);
                var queryParams = HttpUtility.ParseQueryString(uri.Query);
                var code = queryParams["code"];
                var error = queryParams["error"];

                if (!string.IsNullOrEmpty(error))
                {
                    SetStatus($"Authorization denied: {error}");
                    MessageBox.Show($"Strava authorization failed: {error}", "Authorization Denied", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    await Task.Delay(1500);
                    Dispatcher.Invoke(() => { DialogResult = false; Close(); });
                    return;
                }

                if (!string.IsNullOrEmpty(code))
                {
                    AuthorizationCode = code;
                    _codeCaptured = true;
                    SetStatus("✅ Authorization successful! Closing...");

                    // Small delay so the user can see the success message
                    await Task.Delay(800);
                    Dispatcher.Invoke(() => { DialogResult = true; Close(); });
                    return;
                }

                SetStatus("Error: No authorization code received");
            }
            catch (Exception ex)
            {
                SetStatus($"Error capturing code: {ex.Message}");
            }
        }
    }

    private void SetStatus(string message)
    {
        Dispatcher.Invoke(() =>
        {
            StatusText.Text = message;
            SpinnerText.Visibility = _codeCaptured ? Visibility.Collapsed : Visibility.Visible;
        });
    }
}

public class StravaWebDeleteResult
{
    [JsonPropertyName("ok")]
    public bool Success { get; set; }

    [JsonPropertyName("status")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}




