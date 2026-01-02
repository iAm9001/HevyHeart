using System.Text.Json.Serialization;

namespace HevyHeartModels.Hevy.V1;

/// <summary>
/// Represents a workout in the Hevy V1 API workouts list response.
/// Contains summary information about a workout including exercises and basic metadata.
/// </summary>
public class HevyWorkout
{
    /// <summary>
    /// Gets or sets the unique identifier for the workout.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the routine this workout is based on, if applicable.
    /// Empty string if the workout is not part of a routine.
    /// </summary>
    [JsonPropertyName("routine_id")]
    public string RoutineId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title or name of the workout.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

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
    /// Gets or sets a value indicating whether the workout is marked as private.
    /// Private workouts are not shared publicly or with followers.
    /// </summary>
    [JsonPropertyName("is_private")]
    public bool IsPrivate { get; set; }

    /// <summary>
    /// Gets or sets the list of exercises performed in this workout.
    /// Each exercise contains sets and other exercise-specific data.
    /// </summary>
    [JsonPropertyName("exercises")]
    public List<HevyExercise> Exercises { get; set; } = new();
}

/// <summary>
/// Represents an exercise within a Hevy workout list response.
/// Contains the exercise name, template reference, and all sets performed.
/// </summary>
public class HevyExercise
{
    /// <summary>
    /// Gets or sets the name or title of the exercise (e.g., "Bench Press", "Squat").
    /// </summary>
    [JsonPropertyName("title")]
    public virtual string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the exercise template this exercise is based on.
    /// Links to the exercise definition in Hevy's exercise database.
    /// </summary>
    [JsonPropertyName("exercise_template_id")]
    public string ExerciseTemplateId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of sets performed for this exercise.
    /// Each set contains weight, reps, duration, or other performance metrics.
    /// </summary>
    [JsonPropertyName("sets")]
    public List<HevySet> Sets { get; set; } = new();
}

/// <summary>
/// Represents a single set within an exercise in a Hevy workout.
/// Contains performance data such as weight, reps, distance, duration, and completion time.
/// Properties are marked as virtual to allow for inheritance and customization in derived classes.
/// </summary>
public class HevySet
{
    /// <summary>
    /// Gets or sets the zero-based index indicating the order of this set within the exercise.
    /// </summary>
    [JsonPropertyName("index")]
    public virtual int Index { get; set; }

    /// <summary>
    /// Gets or sets the type of set (e.g., "normal", "warmup", "failure", "dropset").
    /// </summary>
    [JsonPropertyName("type")]
    public virtual string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the weight used for this set in kilograms.
    /// Null if weight is not applicable to this set (e.g., bodyweight exercises).
    /// </summary>
    [JsonPropertyName("weight_kg")]
    public virtual double? WeightKg { get; set; }

    /// <summary>
    /// Gets or sets the number of repetitions performed in this set.
    /// Null if reps are not applicable to this set (e.g., timed exercises).
    /// </summary>
    [JsonPropertyName("reps")]
    public virtual int? Reps { get; set; }

    /// <summary>
    /// Gets or sets the distance covered in this set in meters.
    /// Null if distance is not applicable to this set.
    /// Typically used for cardio or running exercises.
    /// </summary>
    [JsonPropertyName("distance_meters")]
    public virtual double? DistanceMeters { get; set; }

    /// <summary>
    /// Gets or sets the duration of this set in seconds.
    /// Null if duration is not applicable to this set.
    /// Typically used for timed holds or cardio exercises.
    /// </summary>
    [JsonPropertyName("duration_seconds")]
    public virtual int? DurationSeconds { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this set was completed.
    /// Null if the set has not been completed or if completion time was not recorded.
    /// </summary>
    [JsonPropertyName("completed_at")]
    public virtual string? CompletedAt { get; set; }
}

/// <summary>
/// Response model for the Hevy V1 API workouts list endpoint.
/// Contains a paginated list of workouts and pagination metadata.
/// </summary>
public class HevyWorkoutsResponse
{
    /// <summary>
    /// Gets or sets the list of workouts returned in this page of results.
    /// </summary>
    [JsonPropertyName("workouts")]
    public List<HevyWorkout> Workouts { get; set; } = new();

    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the total number of pages available.
    /// Use this to determine if more pages can be fetched.
    /// </summary>
    [JsonPropertyName("page_count")]
    public int PageCount { get; set; }
}