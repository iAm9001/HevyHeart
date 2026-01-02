using System.Text.Json.Serialization;

namespace HevyHeartModels.Hevy.V2;

/// <summary>
/// Request model for updating an existing workout via the Hevy V2 API.
/// Used to modify workout details, exercises, and sets after a workout has been created.
/// </summary>
public class UpdateWorkout
{
    /// <summary>
    /// Gets or sets the workout update details containing exercises and timing information.
    /// </summary>
    [JsonPropertyName("workoutUpdate")]
    public WorkoutUpdate WorkoutUpdate { get; set; } = new();
}

/// <summary>
/// Contains the detailed update information for a workout.
/// Includes the list of exercises and the workout start/end time modifications.
/// </summary>
public class WorkoutUpdate
{
    /// <summary>
    /// Gets or sets the list of exercises to update in the workout.
    /// This replaces the existing exercises in the workout.
    /// </summary>
    [JsonPropertyName("exercises")]
    public List<UpdateExercise> Exercises { get; set; } = new();

    /// <summary>
    /// Gets or sets the updated start and end time information for the workout.
    /// </summary>
    [JsonPropertyName("start_and_end_time")]
    public StartAndEndTime StartAndEndTime { get; set; } = new();
}

/// <summary>
/// Represents an exercise in a workout update request.
/// Contains the exercise details and all sets to be updated or added.
/// </summary>
public class UpdateExercise
{
    /// <summary>
    /// Gets or sets the name or title of the exercise (e.g., "Bench Press", "Squat").
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the exercise template this exercise is based on.
    /// Links to the exercise definition in Hevy's exercise database.
    /// </summary>
    [JsonPropertyName("exercise_template_id")]
    public string ExerciseTemplateId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rest timer duration in seconds between sets for this exercise.
    /// </summary>
    [JsonPropertyName("rest_timer_seconds")]
    public int RestTimerSeconds { get; set; }

    /// <summary>
    /// Gets or sets optional notes or comments about this exercise.
    /// </summary>
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether volume doubling is enabled for this exercise.
    /// When enabled, each rep counts as double volume (typically for exercises performed on each side).
    /// </summary>
    [JsonPropertyName("volume_doubling_enabled")]
    public bool VolumeDoublingEnabled { get; set; }

    /// <summary>
    /// Gets or sets the list of sets for this exercise.
    /// This replaces the existing sets for this exercise in the workout.
    /// </summary>
    [JsonPropertyName("sets")]
    public List<UpdateSet> Sets { get; set; } = new();
}

/// <summary>
/// Represents a single set in an exercise update request.
/// Contains the performance metrics to be updated or added for the set.
/// </summary>
public class UpdateSet
{
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
    /// Null if weight is not applicable to this set (e.g., bodyweight exercises).
    /// </summary>
    [JsonPropertyName("weight_kg")]
    public double? WeightKg { get; set; }

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

    /// <summary>
    /// Gets or sets the duration of this set in seconds.
    /// Null if duration is not applicable to this set.
    /// Typically used for timed holds or cardio exercises.
    /// </summary>
    [JsonPropertyName("duration_seconds")]
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this set was completed.
    /// Null if the set has not been completed or if completion time should not be updated.
    /// </summary>
    [JsonPropertyName("completed_at")]
    public string? CompletedAt { get; set; }
}

/// <summary>
/// Contains the start and end time information for a workout.
/// Times are represented as Unix timestamps in seconds.
/// </summary>
public class StartAndEndTime
{
    /// <summary>
    /// Gets or sets the Unix timestamp (in seconds) when the workout started.
    /// </summary>
    [JsonPropertyName("start_time")]
    public long StartTime { get; set; }

    /// <summary>
    /// Gets or sets the Unix timestamp (in seconds) when the workout ended.
    /// </summary>
    [JsonPropertyName("end_time")]
    public long EndTime { get; set; }
}