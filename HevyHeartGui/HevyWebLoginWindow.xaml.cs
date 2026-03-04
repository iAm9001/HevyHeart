using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace HevyHeartGui;

/// <summary>
/// Embeds the Hevy web login page and automatically captures the OAuth tokens
/// from the auth2.0-token cookie once the user has successfully logged in.
///
/// Mirrors the pywebview approach from "Under The Bar" (SteveG/underthebar):
///   - Opens https://hevy.com/login in an embedded browser
///   - Watches for the API login response at https://api.hevyapp.com/login
///   - Reads the auth2.0-token cookie, URL-decodes it, extracts the tokens
///   - Closes automatically when tokens are captured
/// </summary>
public partial class HevyWebLoginWindow : Window
{
    /// <summary>The captured OAuth access token. Null if login was not completed.</summary>
    public string? AccessToken { get; private set; }

    /// <summary>The captured OAuth refresh token. Null if login was not completed.</summary>
    public string? RefreshToken { get; private set; }

    /// <summary>The expiry timestamp for the access token (ISO 8601). Null if login was not completed.</summary>
    public string? ExpiresAt { get; private set; }

    private bool _tokensCaptured;

    public HevyWebLoginWindow()
    {
        InitializeComponent();
        Loaded += OnWindowLoaded;
    }

    private async void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await WebView.EnsureCoreWebView2Async();

            // Suppress the default "new window" behaviour so OAuth pop-ups open inline
            WebView.CoreWebView2.NewWindowRequested += (s, args) =>
            {
                args.Handled = true;
                WebView.CoreWebView2.Navigate(args.Uri);
            };

            // Watch every HTTP response — same as pywebview's on_response
            WebView.CoreWebView2.WebResourceResponseReceived += OnWebResourceResponseReceived;
        }
        catch (Exception ex)
        {
            SetStatus($"Error initialising browser: {ex.Message}");
        }
    }

    /// <summary>
    /// Fires on every HTTP response the embedded browser receives.
    /// When the Hevy login API responds, we check the cookie store for the auth token.
    /// This is the direct C# equivalent of the Python on_response() in underthebar.py.
    /// </summary>
    private async void OnWebResourceResponseReceived(object? sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
    {
        if (_tokensCaptured) return;

        // Mirror the Python check: if response.url == 'https://api.hevyapp.com/login'
        var url = e.Request.Uri;
        if (!url.StartsWith("https://api.hevyapp.com/login", StringComparison.OrdinalIgnoreCase))
            return;

        SetStatus("Login response detected — capturing tokens...");

        try
        {
            // Mirror: cookies = window.get_cookies()
            //         for c in cookies: if "auth2.0-token" in c
            var cookies = await WebView.CoreWebView2.CookieManager.GetCookiesAsync("https://hevy.com");

            foreach (var cookie in cookies)
            {
                if (cookie.Name != "auth2.0-token") continue;

                // The cookie value is URL-encoded JSON — decode it (mirror: urllib.parse.unquote)
                var decoded = HttpUtility.UrlDecode(cookie.Value);
                var json = JsonDocument.Parse(decoded);
                var root = json.RootElement;

                AccessToken = root.GetProperty("access_token").GetString();
                RefreshToken = root.GetProperty("refresh_token").GetString();

                // expires_at may or may not be present
                if (root.TryGetProperty("expires_at", out var expiresEl))
                    ExpiresAt = expiresEl.GetString();

                if (!string.IsNullOrEmpty(AccessToken) && !string.IsNullOrEmpty(RefreshToken))
                {
                    _tokensCaptured = true;
                    SetStatus("✅ Tokens captured! Closing...");

                    // Small delay so the user can see the success message
                    await Task.Delay(800);
                    Dispatcher.Invoke(() => { DialogResult = true; Close(); });
                    return;
                }
            }

            // Cookie not yet present — login may still be processing
            SetStatus("Waiting for auth cookie...");
        }
        catch (Exception ex)
        {
            SetStatus($"Error reading cookie: {ex.Message}");
        }
    }

    private void SetStatus(string message)
    {
        Dispatcher.Invoke(() =>
        {
            StatusText.Text = message;
            SpinnerText.Visibility = _tokensCaptured ? Visibility.Collapsed : Visibility.Visible;
        });
    }
}
