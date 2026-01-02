using System.Text.Json.Serialization;

namespace HevyHeartModels.Hevy.V1;

/// <summary>
/// Response model for Hevy V1 API get workout endpoint.
/// Represents a complete workout including metadata, exercises, and sets.
/// </summary>
public class GetWorkoutResponse
{
    /// <summary>
    /// Gets or sets the unique identifier for the workout.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title or name of the workout.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the routine this workout is based on, if applicable.
    /// Empty string if the workout is not part of a routine.
    /// </summary>
    [JsonPropertyName("routine_id")]
    public string RoutineId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description or notes for the workout.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the workout started.
    /// </summary>
    [JsonPropertyName("start_time")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the workout ended.
    /// </summary>
    [JsonPropertyName("end_time")]
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the workout was last updated.
    /// </summary>
    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the workout was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of exercises performed in this workout.
    /// Each exercise contains sets and other exercise-specific data.
    /// </summary>
    [JsonPropertyName("exercises")]
    public List<V1Exercise> Exercises { get; set; } = new();
}

/// <summary>
/// Represents an exercise within a Hevy V1 workout.
/// Contains the exercise metadata, notes, and all sets performed.
/// </summary>
public class V1Exercise
{
    /// <summary>
    /// Gets or sets the zero-based index indicating the order of this exercise in the workout.
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets the name or title of the exercise (e.g., "Bench Press", "Squat").
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional notes or comments about this exercise.
    /// </summary>
    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the exercise template this exercise is based on.
    /// Links to the exercise definition in Hevy's exercise database.
    /// </summary>
    [JsonPropertyName("exercise_template_id")]
    public string ExerciseTemplateId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the superset identifier if this exercise is part of a superset.
    /// Null if the exercise is not part of a superset.
    /// Exercises with the same superset ID are performed together as a superset.
    /// </summary>
    [JsonPropertyName("superset_id")]
    public string? SupersetId { get; set; }

    /// <summary>
    /// Gets or sets the list of sets performed for this exercise.
    /// Each set contains weight, reps, duration, or other performance metrics.
    /// </summary>
    [JsonPropertyName("sets")]
    public List<V1Set> Sets { get; set; } = new();
}

/// <summary>
/// Represents a single set within an exercise in a Hevy V1 workout.
/// Contains performance data such as weight, reps, distance, duration, and RPE.
/// </summary>
public class V1Set
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
    /// Gets or sets the distance covered in this set in meters.
    /// Null if distance is not applicable to this set.
    /// Typically used for cardio or running exercises.
    /// </summary>
    [JsonPropertyName("distance_meters")]
    public double? DistanceMeters { get; set; }

    /// <summary>
    /// Gets or sets the duration of this set in seconds.
    /// Null if duration is not applicable to this set.
    /// Typically used for timed holds or cardio exercises.
    /// </summary>
    [JsonPropertyName("duration_seconds")]
    public int? DurationSeconds { get; set; }

    /// <summary>
    /// Gets or sets the Rate of Perceived Exertion (RPE) for this set.
    /// Scale typically ranges from 1-10, where 10 is maximum effort.
    /// Null if RPE was not recorded for this set.
    /// </summary>
    [JsonPropertyName("rpe")]
    public double? Rpe { get; set; }

    /// <summary>
    /// Gets or sets a custom metric value for this set, if applicable.
    /// The structure of this object depends on the exercise template configuration.
    /// Null if no custom metric is defined for this set.
    /// </summary>
    [JsonPropertyName("custom_metric")]
    public object? CustomMetric { get; set; }
}

