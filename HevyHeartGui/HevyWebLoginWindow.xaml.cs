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
    private bool _isPolling;
    private readonly object _pollingLock = new object();

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

            // Clear all browser cache and cookies to force fresh login
            await ClearBrowserDataAsync();

            // Suppress the default "new window" behaviour so OAuth pop-ups open inline
            WebView.CoreWebView2.NewWindowRequested += (s, args) =>
            {
                args.Handled = true;
                WebView.CoreWebView2.Navigate(args.Uri);
            };

            // Watch every HTTP response — same as pywebview's on_response
            WebView.CoreWebView2.WebResourceResponseReceived += OnWebResourceResponseReceived;
            
            // Also watch navigation events to detect OAuth redirects back to Hevy
            WebView.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
        }
        catch (Exception ex)
        {
            SetStatus($"Error initialising browser: {ex.Message}");
        }
    }

    /// <summary>
    /// Fires when navigation completes. Used to detect when Google OAuth redirects back to Hevy
    /// and check if the authentication cookie has been set.
    /// </summary>
    private async void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (_tokensCaptured) return;

        try
        {
            var currentUrl = WebView.CoreWebView2.Source;
            
            // Check if we're back on a Hevy domain after OAuth (Google, Apple, etc.)
            // This includes intermediate loading/redirect pages
            if (currentUrl.Contains("hevy.com") || currentUrl.Contains("hevyapp.com"))
            {
                SetStatus("Checking for authentication tokens...");
                
                // Immediately check once
                await CheckForAuthCookieAsync();
                
                // Start aggressive polling for the cookie - it might take a few moments
                // after the page loads for the JavaScript to set the cookie, or there
                // might be additional redirects before the cookie is set
                bool shouldStartPolling = false;
                lock (_pollingLock)
                {
                    if (!_isPolling && !_tokensCaptured)
                    {
                        _isPolling = true;
                        shouldStartPolling = true;
                    }
                }
                
                if (shouldStartPolling)
                {
                    _ = Task.Run(async () => await PollForAuthCookieAsync());
                }
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Navigation check error: {ex.Message}");
        }
    }

    /// <summary>
    /// Continuously polls for the auth cookie until found or timeout.
    /// This handles cases where the cookie is set by JavaScript after page load.
    /// </summary>
    private async Task PollForAuthCookieAsync()
    {
        const int maxAttempts = 240; // 60 seconds total (240 * 250ms)
        const int delayMs = 250;
        
        for (int i = 0; i < maxAttempts && !_tokensCaptured; i++)
        {
            await Task.Delay(delayMs);
            await CheckForAuthCookieAsync();
            
            if (_tokensCaptured)
            {
                lock (_pollingLock)
                {
                    _isPolling = false;
                }
                return;
            }
        }
        
        lock (_pollingLock)
        {
            _isPolling = false;
        }
        
        if (!_tokensCaptured)
        {
            Dispatcher.Invoke(() =>
            {
                SetStatus("⚠️ Timeout waiting for auth cookie. Try closing and reopening.");
            });
        }
    }

    /// <summary>
    /// Clears only Hevy-specific browser data (cookies, storage) to force a fresh login.
    /// Does not clear other websites' data.
    /// </summary>
    private async Task ClearBrowserDataAsync()
    {
        try
        {
            SetStatus("Clearing Hevy authentication data...");

            // Clear all cookies from Hevy domains only
            var hevyCookies = await WebView.CoreWebView2.CookieManager.GetCookiesAsync("https://hevy.com");
            foreach (var cookie in hevyCookies)
            {
                WebView.CoreWebView2.CookieManager.DeleteCookie(cookie);
            }

            var apiCookies = await WebView.CoreWebView2.CookieManager.GetCookiesAsync("https://api.hevyapp.com");
            foreach (var cookie in apiCookies)
            {
                WebView.CoreWebView2.CookieManager.DeleteCookie(cookie);
            }

            var appCookies = await WebView.CoreWebView2.CookieManager.GetCookiesAsync("https://app.hevyapp.com");
            foreach (var cookie in appCookies)
            {
                WebView.CoreWebView2.CookieManager.DeleteCookie(cookie);
            }

            // Clear browsing data for the profile
            // Note: This clears cache/storage globally, but we've already cleared Hevy cookies specifically above
            try
            {
                await WebView.CoreWebView2.Profile.ClearBrowsingDataAsync(
                    CoreWebView2BrowsingDataKinds.CacheStorage |
                    CoreWebView2BrowsingDataKinds.DiskCache |
                    CoreWebView2BrowsingDataKinds.IndexedDb |
                    CoreWebView2BrowsingDataKinds.LocalStorage |
                    CoreWebView2BrowsingDataKinds.ServiceWorkers |
                    CoreWebView2BrowsingDataKinds.WebSql);
            }
            catch
            {
                // If clearing fails, continue anyway
            }

            SetStatus("Hevy data cleared. Waiting for login...");
        }
        catch (Exception ex)
        {
            // If clearing fails, continue anyway - better to try logging in than to fail completely
            SetStatus($"Warning: Could not clear cache: {ex.Message}");
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

        var url = e.Request.Uri;
        
        // Check for various authentication endpoints:
        // - /login (username/password)
        // - /auth/* (OAuth callbacks)
        // - /account (post-login navigation)
        // - /session (session establishment)
        if (url.Contains("api.hevyapp.com/login") || 
            url.Contains("api.hevyapp.com/auth") ||
            url.Contains("api.hevyapp.com/account") ||
            url.Contains("api.hevyapp.com/session") ||
            url.Contains("hevyapp.com/oauth") ||
            url.Contains("hevy.com/oauth"))
        {
            SetStatus("Authentication response detected — checking for tokens...");
            
            // Give the browser a moment to set the cookie
            await Task.Delay(300);
            await CheckForAuthCookieAsync();
        }
    }

    /// <summary>
    /// Checks all Hevy-related domains for the auth2.0-token cookie and extracts the tokens if found.
    /// </summary>
    private async Task CheckForAuthCookieAsync()
    {
        if (_tokensCaptured) return;

        // Ensure we're on the UI thread to access WebView2
        if (!Dispatcher.CheckAccess())
        {
            await Dispatcher.InvokeAsync(async () => await CheckForAuthCookieAsync());
            return;
        }

        try
        {
            // Check all possible Hevy domains where the cookie might be set
            var domains = new[] { "https://hevy.com", "https://app.hevyapp.com", "https://api.hevyapp.com" };
            
            var currentUrl = WebView.CoreWebView2?.Source ?? "unknown";
            var totalCookiesFound = 0;
            
            foreach (var domain in domains)
            {
                if (_tokensCaptured) return;
                
                var cookies = await WebView.CoreWebView2.CookieManager.GetCookiesAsync(domain);
                totalCookiesFound += cookies.Count;

                foreach (var cookie in cookies)
                {
                    if (cookie.Name != "auth2.0-token") continue;

                    try
                    {
                        // The cookie value is URL-encoded JSON — decode it (mirror: urllib.parse.unquote)
                        var decoded = HttpUtility.UrlDecode(cookie.Value);
                        var json = JsonDocument.Parse(decoded);
                        var root = json.RootElement;

                        var accessToken = root.GetProperty("access_token").GetString();
                        var refreshToken = root.GetProperty("refresh_token").GetString();

                        // expires_at may or may not be present
                        string? expiresAt = null;
                        if (root.TryGetProperty("expires_at", out var expiresEl))
                            expiresAt = expiresEl.GetString();

                        if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
                        {
                            AccessToken = accessToken;
                            RefreshToken = refreshToken;
                            ExpiresAt = expiresAt;
                            
                            _tokensCaptured = true;
                            lock (_pollingLock)
                            {
                                _isPolling = false;
                            }
                            
                            SetStatus("✅ Tokens captured! Closing...");
                            
                            // Small delay so the user can see the success message
                            await Task.Delay(800);
                            DialogResult = true;
                            Close();
                            return;
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        // Cookie exists but isn't valid JSON yet - might still be loading
                        SetStatus($"Found cookie but invalid JSON: {jsonEx.Message}");
                        continue;
                    }
                }
            }
            
            // Debug: Show what we're seeing
            if (totalCookiesFound == 0 && !_tokensCaptured)
            {
                SetStatus($"No cookies found yet (on {currentUrl.Substring(0, Math.Min(50, currentUrl.Length))})...");
            }
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
