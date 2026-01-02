using System.Net;
using System.Text;

namespace HevyHeartConsole.Infrastructure;

/// <summary>
/// Provides an HTTP callback server for handling OAuth authorization callbacks.
/// This server listens for incoming HTTP requests and extracts the authorization code from the callback URL.
/// </summary>
public class CallbackServer
{
    /// <summary>
    /// The HTTP listener that handles incoming requests on the specified redirect URI.
    /// </summary>
    private readonly HttpListener _listener;
    
    /// <summary>
    /// The redirect URI where the OAuth provider will send the authorization callback.
    /// </summary>
    private readonly string _redirectUri;
    
    /// <summary>
    /// A task completion source that completes when the authorization code is received.
    /// </summary>
    private readonly TaskCompletionSource<string> _authCodeTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="CallbackServer"/> class.
    /// </summary>
    /// <param name="redirectUri">The URI where the server will listen for callbacks.</param>
    public CallbackServer(string redirectUri)
    {
        _redirectUri = redirectUri;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"{redirectUri}/");
        _authCodeTask = new TaskCompletionSource<string>();
    }

    /// <summary>
    /// Starts the HTTP listener and waits asynchronously for the OAuth callback with an authorization code.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the authorization code extracted from the callback.</returns>
    public async Task<string> StartAndWaitForCallbackAsync()
    {
        _listener.Start();
        Console.WriteLine($"Callback server started at {_redirectUri}");
        Console.WriteLine("Waiting for Strava authorization callback...");

        _ = Task.Run(async () =>
        {
            try
            {
                while (_listener.IsListening)
                {
                    var context = await _listener.GetContextAsync();
                    await HandleRequestAsync(context);
                }
            }
            catch (Exception ex) when (ex is ObjectDisposedException || ex is HttpListenerException)
            {
                // Expected when stopping the listener
            }
        });

        return await _authCodeTask.Task;
    }

    /// <summary>
    /// Handles incoming HTTP requests and processes OAuth callback responses.
    /// Extracts the authorization code from the callback URL and returns an HTML response to the user.
    /// </summary>
    /// <param name="context">The HTTP listener context containing the request and response objects.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task HandleRequestAsync(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;

        if (request.Url?.AbsolutePath == "/callback")
        {
            var query = request.Url.Query;
            var code = ExtractCodeFromQuery(query);

            string responseString;
            if (!string.IsNullOrEmpty(code))
            {
                responseString = @"
                    <html>
                        <body>
                            <h1>Authorization Successful!</h1>
                            <p>You can close this window and return to the application.</p>
                            <script>window.close();</script>
                        </body>
                    </html>";
                _authCodeTask.SetResult(code);
            }
            else
            {
                responseString = @"
                    <html>
                        <body>
                            <h1>Authorization Failed</h1>
                            <p>No authorization code received.</p>
                        </body>
                    </html>";
                _authCodeTask.SetException(new Exception("No authorization code received"));
            }

            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentType = "text/html";
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();
        }

        Stop();
    }

    /// <summary>
    /// Extracts the authorization code from the query string of the OAuth callback URL.
    /// </summary>
    /// <param name="query">The query string portion of the callback URL.</param>
    /// <returns>The authorization code if found; otherwise, null.</returns>
    private static string? ExtractCodeFromQuery(string query)
    {
        if (string.IsNullOrEmpty(query)) return null;

        var queryParams = query.TrimStart('?').Split('&');
        var codeParam = queryParams.FirstOrDefault(p => p.StartsWith("code="));
        return codeParam?.Split('=')[1];
    }

    /// <summary>
    /// Stops the HTTP listener and closes all associated resources.
    /// </summary>
    public void Stop()
    {
        if (_listener.IsListening)
        {
            _listener.Stop();
            _listener.Close();
        }
    }
}
