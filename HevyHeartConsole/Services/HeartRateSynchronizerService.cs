using HevyHeartModels.Internal;
using HevyHeartModels.Hevy.V2;
using HevyHeartModels.Strava;

namespace HevyHeartConsole.Services;

/// <summary>
/// Provides functionality to synchronize heart rate data from Strava activities with Hevy workouts.
/// This service interpolates Strava heart rate streams to match the duration and timing of Hevy workouts.
/// </summary>
public class HeartRateSynchronizerService
{
    /// <summary>
    /// Synchronizes heart rate data from a Strava activity to match a Hevy workout's timeline.
    /// The method interpolates Strava heart rate samples at 1-second intervals across the Hevy workout duration.
    /// </summary>
    /// <param name="stravaActivity">The detailed Strava activity containing calorie and workout information.</param>
    /// <param name="heartRateStream">The Strava heart rate stream containing BPM data points.</param>
    /// <param name="hevyWorkout">The Hevy workout response model containing start time, end time, and workout details.</param>
    /// <returns>A <see cref="Biometrics"/> object containing synchronized heart rate samples and total calories.</returns>
    public static Biometrics SynchronizeHeartRateData(
        StravaDetailedActivity stravaActivity,
        StravaHeartRateStream heartRateStream,
        GetWorkoutResponseModel hevyWorkout)
    {
        // Convert Hevy DateTime timestamps to milliseconds for calculation
        var hevyStartMs = ((DateTimeOffset)hevyWorkout.GetWorkoutResponseV1.StartTime).ToUnixTimeMilliseconds();
        var hevyEndMs = ((DateTimeOffset)hevyWorkout.GetWorkoutResponseV1.EndTime).ToUnixTimeMilliseconds();
        var hevyDurationMs = hevyEndMs - hevyStartMs;

        var heartRateSamples = new List<HeartRateSample>();

        // If we have heart rate data, interpolate it to match Hevy workout duration
        if (heartRateStream.Data.Length > 0)
        {
            // Create samples at 1-second intervals for the duration of the Hevy workout
            var totalSeconds = (int)(hevyDurationMs / 1000);
            
            for (int i = 0; i < totalSeconds; i++)
            {
                var timestampMs = hevyStartMs + (i * 1000);
                
                // Calculate corresponding index in Strava data
                var stravaIndex = (int)((double)i / totalSeconds * heartRateStream.Data.Length);
                stravaIndex = Math.Min(stravaIndex, heartRateStream.Data.Length - 1);
                
                var bpm = heartRateStream.Data[stravaIndex];
                
                heartRateSamples.Add(new HeartRateSample
                {
                    TimestampMs = timestampMs,
                    Bpm = bpm
                });
            }
        }

        return new Biometrics
        {
            TotalCalories = stravaActivity.Calories ?? 0,
            HeartRateSamples = heartRateSamples
        };
    }
}