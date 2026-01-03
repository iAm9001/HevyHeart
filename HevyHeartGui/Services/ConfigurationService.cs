using System.IO;
using System.Text.Json;
using HevyHeartConsole.Config;
using System.Diagnostics;

namespace HevyHeartGui.Services;

/// <summary>
/// Service for validating and saving application configuration.
/// </summary>
public class ConfigurationService
{
    private readonly string _configPath;

    public ConfigurationService()
    {
        _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        Debug.WriteLine($"ConfigurationService initialized with path: {_configPath}");
    }

    /// <summary>
    /// Validates required configuration settings.
    /// </summary>
    /// <param name="config">The configuration to validate.</param>
    /// <param name="missingSettings">List of missing settings.</param>
    /// <returns>True if all required settings are present, false otherwise.</returns>
    public bool ValidateConfiguration(AppConfig config, out List<string> missingSettings)
    {
        missingSettings = new List<string>();

        // Validate Strava settings
        if (string.IsNullOrWhiteSpace(config.Strava.ClientId))
            missingSettings.Add("Strava Client ID");

        if (string.IsNullOrWhiteSpace(config.Strava.ClientSecret))
            missingSettings.Add("Strava Client Secret");

        if (string.IsNullOrWhiteSpace(config.Strava.RedirectUri))
            missingSettings.Add("Strava Redirect URI");

        // Validate Hevy settings - only API key is required
        // Username/password are optional and can be entered in the main window
        if (string.IsNullOrWhiteSpace(config.Hevy.ApiKey))
            missingSettings.Add("Hevy API Key");

        return missingSettings.Count == 0;
    }

    /// <summary>
    /// Saves the configuration back to appsettings.json.
    /// </summary>
    /// <param name="config">The configuration to save.</param>
    public void SaveConfiguration(AppConfig config)
    {
        try
        {
            Debug.WriteLine($"SaveConfiguration called");
            Debug.WriteLine($"Target path: {_configPath}");
            Debug.WriteLine($"Strava ClientId: '{config.Strava.ClientId}'");
            Debug.WriteLine($"Hevy ApiKey: {(string.IsNullOrEmpty(config.Hevy.ApiKey) ? "empty" : "***")}");
            
            // Ensure default values are set if missing
            if (string.IsNullOrWhiteSpace(config.Strava.Scope))
                config.Strava.Scope = "read,activity:read_all";
            
            if (string.IsNullOrWhiteSpace(config.Hevy.BaseUrl))
                config.Hevy.BaseUrl = "https://api.hevyapp.com";
            
            if (config.Server.Port == 0)
                config.Server.Port = 8080;
            
            if (string.IsNullOrWhiteSpace(config.Server.Host))
                config.Server.Host = "localhost";

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = null, // Use property names as-is (PascalCase)
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
            };

            var json = JsonSerializer.Serialize(config, options);
            
            Debug.WriteLine($"Serialized JSON length: {json.Length}");
            Debug.WriteLine($"First 500 chars of JSON:");
            Debug.WriteLine(json.Substring(0, Math.Min(500, json.Length)));
            
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Debug.WriteLine($"Creating directory: {directory}");
                Directory.CreateDirectory(directory);
            }

            Debug.WriteLine($"Writing to file...");
            File.WriteAllText(_configPath, json);
            Debug.WriteLine($"File write complete");
            
            // Verify the file was written
            if (File.Exists(_configPath))
            {
                var fileInfo = new FileInfo(_configPath);
                Debug.WriteLine($"File verified - size: {fileInfo.Length} bytes, last modified: {fileInfo.LastWriteTime}");
                
                // Read it back to verify
                var readBack = File.ReadAllText(_configPath);
                Debug.WriteLine($"Read back {readBack.Length} characters from file");
            }
            else
            {
                Debug.WriteLine($"WARNING: File does not exist after write!");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving configuration: {ex}");
            throw new InvalidOperationException($"Failed to save configuration to '{_configPath}': {ex.Message}", ex);
        }
    }
}
