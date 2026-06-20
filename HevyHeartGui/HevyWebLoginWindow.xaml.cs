using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

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
    private Window? _activeOAuthPopup;

    /// <summary>
    /// Milliseconds to wait after the OAuth callback page loads before reading cookies.
    /// The page JavaScript may still be writing cookies when NavigationCompleted fires.
    /// </summary>
    private const int CookiePropagationDelayMs = 500;

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

            // Route OAuth provider popups (Google, Apple) to a proper secondary WebView2 window
            // that shares this window's CoreWebView2Environment so both windows share one cookie
            // store. This preserves the window.opener contract expected by Hevy's JavaScript
            // login flow; without it the auth2.0-token cookie is never set (issue #30).
            WebView.CoreWebView2.NewWindowRequested += OnNewWindowRequested;

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
    /// Fires when the Hevy login page requests a new browser window (e.g. "Continue with Google").
    ///
    /// OAuth provider popups (Google, Apple) are opened in a secondary WPF window whose
    /// WebView2 is initialised with <em>the same <see cref="CoreWebView2Environment"/></em> as
    /// this window.  Sharing an environment means sharing one cookie store: any cookie set by the
    /// OAuth callback page inside the popup (e.g. <c>auth2.0-token</c>) is immediately accessible
    /// through this window's <see cref="CoreWebView2.CookieManager"/>, so the existing polling
    /// logic finds the token without any changes.
    ///
    /// The previous approach of redirecting the popup to the main WebView navigated the main
    /// WebView away from <c>hevy.com/login</c>, which destroyed the JavaScript context that was
    /// waiting for a <c>window.opener</c> signal from its popup.  Without that signal Hevy's
    /// front-end never set the auth cookie, causing the window to spin forever (issue #30).
    /// </summary>
    private async void OnNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs args)
    {
        var uri = args.Uri;

        // Identify OAuth provider URLs that use a popup-based flow.
        // Parse the URI properly to avoid subdomain-spoofing false positives
        // (e.g. 'evil.com/accounts.google.com' must NOT match).
        if (!IsOAuthProviderUri(uri))
        {
            // Non-OAuth popups (Hevy-internal navigation): keep existing inline behaviour
            args.Handled = true;
            WebView.CoreWebView2.Navigate(uri);
            return;
        }

        // Obtain a deferral so we can complete setup asynchronously before WebView2
        // uses the new window.
        var deferral = args.GetDeferral();
        args.Handled = true;

        // Declared outside try so the catch block can dispose it if initialisation fails
        var popupWebView = new WebView2();
        try
        {
            var providerName = GetOAuthProviderName(uri);
            SetStatus($"Opening {providerName} sign-in window...");

            var popupWindow = new Window
            {
                Title = $"Sign in with {providerName}",
                Content = popupWebView,
                Width = 520,
                Height = 660,
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowStyle = WindowStyle.SingleBorderWindow
            };

            _activeOAuthPopup = popupWindow;

            popupWindow.Closed += (s, e) =>
            {
                if (_activeOAuthPopup == popupWindow)
                    _activeOAuthPopup = null;
                if (!_tokensCaptured)
                    SetStatus("Sign-in window closed. Waiting for authentication...");
                // Explicitly dispose the WebView2 control to release CoreWebView2 resources
                popupWebView.Dispose();
            };

            popupWindow.Show();

            // CRITICAL: pass the parent's environment so both WebViews share one cookie store.
            await popupWebView.EnsureCoreWebView2Async(WebView.CoreWebView2.Environment);

            // Redirect any sub-popups within the provider flow (e.g. 2FA) inline inside
            // the popup window rather than opening yet another window.
            popupWebView.CoreWebView2.NewWindowRequested += (s, a) =>
            {
                a.Handled = true;
                popupWebView.CoreWebView2.Navigate(a.Uri);
            };

            // When the popup navigates back to a Hevy domain the OAuth callback has
            // completed and the cookie should now be in the shared store.
            popupWebView.CoreWebView2.NavigationCompleted += async (s, e2) =>
            {
                if (_tokensCaptured) return;

                var url = popupWebView.CoreWebView2.Source;
                if (IsHevyDomain(url))
                {
                    SetStatus("OAuth returned to Hevy — checking for tokens...");

                    // Brief pause to allow the page to finish setting cookies
                    await Task.Delay(CookiePropagationDelayMs);
                    await CheckForAuthCookieAsync();

                    bool shouldPoll;
                    lock (_pollingLock)
                    {
                        shouldPoll = !_isPolling && !_tokensCaptured;
                        if (shouldPoll) _isPolling = true;
                    }
                    if (shouldPoll)
                        _ = Task.Run(() => PollForAuthCookieAsync());
                }
            };

            // Provide the initialised CoreWebView2 as the popup target
            args.NewWindow = popupWebView.CoreWebView2;
        }
        catch (Exception ex)
        {
            SetStatus($"Could not open OAuth popup ({ex.Message}) — trying inline.");
            // Dispose the WebView2 that was created but never successfully initialised
            popupWebView.Dispose();
            // Fallback: original inline navigation so the user can still attempt login
            WebView.CoreWebView2.Navigate(uri);
        }
        finally
        {
            deferral.Complete();
        }
    }

    private async void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        if (_tokensCaptured) return;

        try
        {
            var currentUrl = WebView.CoreWebView2.Source;
            
            // Check if we're back on a Hevy domain after OAuth (Google, Apple, etc.)
            // This includes intermediate loading/redirect pages
            if (IsHevyDomain(currentUrl))
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
                if (_activeOAuthPopup != null)
                    SetStatus("Waiting for sign-in to complete in the popup window...");
                else
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
                            
                            // Close the OAuth provider popup if it is still open
                            _activeOAuthPopup?.Close();
                            _activeOAuthPopup = null;
                            
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
            
            // Suppress the noisy "no cookies yet" status during initial page load —
            // the meaningful status updates come from the navigation/response event handlers.
            if (totalCookiesFound == 0 && !_tokensCaptured)
            {
                SetStatus("Waiting for Hevy authentication...");
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

    /// <summary>
    /// Returns true when <paramref name="rawUri"/> is a recognised OAuth provider URL.
    /// Uses proper URI parsing to avoid false positives from path-embedded domain names
    /// (e.g. <c>https://evil.com/accounts.google.com</c> must NOT match).
    /// </summary>
    private static bool IsOAuthProviderUri(string rawUri)
    {
        if (!Uri.TryCreate(rawUri, UriKind.Absolute, out var uri)) return false;
        var host = uri.Host;
        return host == "accounts.google.com" ||
               // Any *.google.com subdomain serving /o/oauth2 is a legitimate Google OAuth endpoint;
               // the dot prefix ensures 'evilgoogle.com' cannot match.
               (host.EndsWith(".google.com", StringComparison.OrdinalIgnoreCase) &&
                uri.AbsolutePath.StartsWith("/o/oauth2", StringComparison.OrdinalIgnoreCase)) ||
               host == "appleid.apple.com";
    }

    /// <summary>
    /// Returns a human-readable provider name for a recognised OAuth URI,
    /// or <c>"OAuth provider"</c> for any unrecognised provider.
    /// </summary>
    private static string GetOAuthProviderName(string rawUri)
    {
        if (!Uri.TryCreate(rawUri, UriKind.Absolute, out var uri)) return "OAuth provider";
        var host = uri.Host;
        if (host == "accounts.google.com" ||
            (host.EndsWith(".google.com", StringComparison.OrdinalIgnoreCase) &&
             uri.AbsolutePath.StartsWith("/o/oauth2", StringComparison.OrdinalIgnoreCase)))
            return "Google";
        if (host == "appleid.apple.com")
            return "Apple";
        return "OAuth provider";
    }

    /// <summary>
    /// Returns true when <paramref name="rawUri"/> belongs to a Hevy domain.
    /// Uses proper URI host comparison to avoid path-spoofing false positives.
    /// </summary>
    private static bool IsHevyDomain(string rawUri)
    {
        if (!Uri.TryCreate(rawUri, UriKind.Absolute, out var uri)) return false;
        var host = uri.Host;
        // Use exact match or dot-prefixed suffix to prevent "evilhevy.com" from matching
        return host == "hevy.com" || host.EndsWith(".hevy.com", StringComparison.OrdinalIgnoreCase) ||
               host == "hevyapp.com" || host.EndsWith(".hevyapp.com", StringComparison.OrdinalIgnoreCase);
    }
}
