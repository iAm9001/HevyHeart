namespace HevyHeartConsole.Infrastructure;

/// <summary>
/// Provides a consistent location for historical workout/activity JSON data that is written to disk
/// for debugging and verification purposes (e.g., raw Hevy/Strava API responses, synchronized biometrics).
/// All such data is written to (and read from) a dedicated subdirectory within the application folder,
/// regardless of build configuration.
/// </summary>
public static class HistoricalDataPaths
{
    /// <summary>
    /// The name of the subdirectory (relative to the application's base directory) where historical
    /// workout data JSON files are written and read from.
    /// </summary>
    public const string DirectoryName = "HistoricalWorkoutData";

    /// <summary>
    /// Gets the full path to the "HistoricalWorkoutData" directory within the application folder,
    /// creating it if it does not already exist.
    /// </summary>
    /// <returns>The absolute path to the historical workout data directory.</returns>
    public static string GetDirectory()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, DirectoryName);
        Directory.CreateDirectory(directory);
        return directory;
    }

    /// <summary>
    /// Resolves the full file path for a given file name within the "HistoricalWorkoutData" directory,
    /// ensuring the directory exists.
    /// </summary>
    /// <param name="fileName">The file name (without directory) to resolve.</param>
    /// <returns>The absolute path combining the historical workout data directory and the file name.</returns>
    public static string ResolvePath(string fileName)
    {
        return Path.Combine(GetDirectory(), fileName);
    }
}

