using System;
using System.Threading.Tasks;
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

    private async Task OnWindowLoadedAsync(string authorizationUrl)
    {
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




