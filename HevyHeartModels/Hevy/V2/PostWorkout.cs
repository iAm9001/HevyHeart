using System.Text.Json.Serialization;

namespace HevyHeartModels.Hevy.V2;

/// <summary>
/// Request model for creating a new workout via the Hevy V2 API.
/// Contains the complete workout data including exercises, biometrics, and sharing preferences.
/// </summary>
public class PostWorkout
{
    /// <summary>
    /// Gets or sets a value indicating whether the workout should be shared to Strava.
    /// When true, the workout will be automatically posted to the user's connected Strava account.
    /// </summary>
    [JsonPropertyName("share_to_strava")]
    public bool ShareToStrava { get; set; }

    /// <summary>
    /// Gets or sets the workout data containing all exercises, biometrics, and metadata.
    /// </summary>
    [JsonPropertyName("workout")]
    public Workout Workout { get; set; } = new();
}

/// <summary>
/// Contains the complete workout data for a create workout request.
/// Includes exercises, timing, biometrics, privacy settings, and device information.
/// </summary>
public class Workout
{
    /// <summary>
    /// Gets or sets a value indicating whether this workout was recorded using an Apple Watch.
    /// </summary>
    [JsonPropertyName("apple_watch")]
    public bool AppleWatch { get; set; }

    /// <summary>
    /// Gets or sets the biometrics data for the workout, including heart rate samples and calories.
    /// </summary>
    [JsonPropertyName("biometrics")]
    public Biometrics Biometrics { get; set; } = new();

    /// <summary>
    /// Gets or sets the optional description or notes for the workout.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Unix timestamp (in seconds) when the workout ended.
    /// </summary>
    [JsonPropertyName("end_time")]
    public long EndTime { get; set; }

    /// <summary>
    /// Gets or sets the list of exercises performed in this workout.
    /// Each exercise contains sets and exercise-specific metadata.
    /// </summary>
    [JsonPropertyName("exercises")]
    public List<Exercise> Exercises { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether biometrics data (heart rate, calories) should be publicly visible.
    /// If false, biometrics are only visible to the workout owner.
    /// </summary>
    [JsonPropertyName("is_biometrics_public")]
    public bool IsBiometricsPublic { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the workout should be marked as private.
    /// Private workouts are not visible to other users or followers.
    /// </summary>
    [JsonPropertyName("is_private")]
    public bool IsPrivate { get; set; }

    /// <summary>
    /// Gets or sets the list of media attachments (photos, videos) to associate with the workout.
    /// Each item's structure depends on the media type being uploaded.
    /// </summary>
    [JsonPropertyName("media")]
    public List<object> Media { get; set; } = new();

    /// <summary>
    /// Gets or sets the identifier of the routine this workout is based on, if applicable.
    /// Empty string if the workout is not part of a routine.
    /// </summary>
    [JsonPropertyName("routine_id")]
    public string RoutineId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Unix timestamp (in seconds) when the workout started.
    /// </summary>
    [JsonPropertyName("start_time")]
    public long StartTime { get; set; }

    /// <summary>
    /// Gets or sets the title or name of the workout.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this workout was recorded using a WearOS watch.
    /// </summary>
    [JsonPropertyName("wearos_watch")]
    public bool WearosWatch { get; set; }

    /// <summary>
    /// Gets or sets an optional workout ID for updating an existing workout.
    /// Empty string when creating a new workout.
    /// </summary>
    [JsonPropertyName("workout_id")]
    public string WorkoutId { get; set; } = string.Empty;
}

/// <summary>
/// Contains biometrics data for a workout including heart rate and calorie information.
/// </summary>
public class Biometrics
{
    /// <summary>
    /// Gets or sets the list of heart rate samples recorded during the workout.
    /// Each sample contains a heart rate measurement and timestamp.
    /// Samples should be at approximately 1-second intervals for best accuracy.
    /// </summary>
    [JsonPropertyName("heart_rate_samples")]
    public List<HeartRateSample> HeartRateSamples { get; set; } = new();

    /// <summary>
    /// Gets or sets the total calories burned during the workout.
    /// </summary>
    [JsonPropertyName("total_calories")]
    public double TotalCalories { get; set; }
}

/// <summary>
/// Represents a single heart rate measurement at a specific point in time during a workout.
/// </summary>
public class HeartRateSample
{
    /// <summary>
    /// Gets or sets the heart rate measurement in beats per minute (bpm).
    /// </summary>
    [JsonPropertyName("bpm")]
    public double Bpm { get; set; }

    /// <summary>
    /// Gets or sets the Unix timestamp in milliseconds when this heart rate sample was recorded.
    /// </summary>
    [JsonPropertyName("timestamp_ms")]
    public long TimestampMs { get; set; }
}

/// <summary>
/// Represents an exercise to be included in a workout creation request.
/// Contains the exercise details and all sets to be recorded.
/// </summary>
public class Exercise
{
    /// <summary>
    /// Gets or sets the identifier of the exercise template this exercise is based on.
    /// Links to the exercise definition in Hevy's exercise database.
    /// </summary>
    [JsonPropertyName("exercise_template_id")]
    public string ExerciseTemplateId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional notes or comments about this exercise.
    /// </summary>
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rest timer duration in seconds between sets for this exercise.
    /// </summary>
    [JsonPropertyName("rest_timer_seconds")]
    public int RestTimerSeconds { get; set; }

    /// <summary>
    /// Gets or sets the list of sets for this exercise.
    /// Each set contains performance metrics like weight, reps, and duration.
    /// </summary>
    [JsonPropertyName("sets")]
    public List<Set> Sets { get; set; } = new();

    /// <summary>
    /// Gets or sets the name or title of the exercise (e.g., "Bench Press", "Squat").
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether volume doubling is enabled for this exercise.
    /// When enabled, each rep counts as double volume (typically for exercises performed on each side).
    /// </summary>
    [JsonPropertyName("volume_doubling_enabled")]
    public bool VolumeDoublingEnabled { get; set; }

    /// <summary>
    /// Gets or sets the superset identifier if this exercise is part of a superset.
    /// Null if the exercise is not part of a superset.
    /// Exercises with the same superset ID are performed together as a superset.
    /// </summary>
    [JsonPropertyName("superset_id")]
    public int? SupersetId { get; set; }
}

/// <summary>
/// Represents a single set to be recorded in an exercise.
/// Contains the performance metrics for the set.
/// </summary>
public class Set
{
    /// <summary>
    /// Gets or sets the timestamp when this set was completed.
    /// </summary>
    [JsonPropertyName("completed_at")]
    public string CompletedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the distance covered in this set in meters.
    /// Used for cardio or running exercises.
    /// </summary>
    [JsonPropertyName("distance_meters")]
    public double DistanceMeters { get; set; }

    /// <summary>
    /// Gets or sets the duration of this set in seconds.
    /// Null if duration is not applicable to this set.
    /// Typically used for timed holds or cardio exercises.
    /// </summary>
    [JsonPropertyName("duration_seconds")]
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Gets or sets the zero-based index indicating the order of this set within the exercise.
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the type of set (e.g., "normal", "warmup", "failure", "dropset").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the weight used for this set in kilograms.
    /// </summary>
    [JsonPropertyName("weight_kg")]
    public double WeightKg { get; set; }

    /// <summary>
    /// Gets or sets the number of repetitions performed in this set.
    /// Null if reps are not applicable to this set (e.g., timed exercises).
    /// </summary>
    [JsonPropertyName("reps")]
    public int? Reps { get; set; }

    /// <summary>
    /// Gets or sets the Rate of Perceived Exertion (RPE) for this set.
    /// Scale typically ranges from 1-10, where 10 is maximum effort.
    /// Null if RPE was not recorded for this set.
    /// </summary>
    [JsonPropertyName("rpe")]
    public double? Rpe { get; set; }
}