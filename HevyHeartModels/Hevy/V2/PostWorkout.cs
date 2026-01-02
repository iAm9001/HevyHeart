using System.Text.Json.Serialization;

namespace HevyHeartModels.Hevy.V2;

/// <summary>
/// Payload model for HevyHeart V2 API post workout endpoint
/// </summary>
public class PostWorkout
{
    [JsonPropertyName("share_to_strava")]
    public bool ShareToStrava { get; set; }

    [JsonPropertyName("workout")]
    public Workout Workout { get; set; } = new();
}

/// <summary>
/// Workout data for PostWorkout payload
/// </summary>
public class Workout
{
    [JsonPropertyName("apple_watch")]
    public bool AppleWatch { get; set; }

    [JsonPropertyName("biometrics")]
    public Biometrics Biometrics { get; set; } = new();

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("end_time")]
    public long EndTime { get; set; }

    [JsonPropertyName("exercises")]
    public List<Exercise> Exercises { get; set; } = new();

    [JsonPropertyName("is_biometrics_public")]
    public bool IsBiometricsPublic { get; set; }

    [JsonPropertyName("is_private")]
    public bool IsPrivate { get; set; }

    [JsonPropertyName("media")]
    public List<object> Media { get; set; } = new();

    [JsonPropertyName("routine_id")]
    public string RoutineId { get; set; } = string.Empty;

    [JsonPropertyName("start_time")]
    public long StartTime { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("wearos_watch")]
    public bool WearosWatch { get; set; }

    [JsonPropertyName("workout_id")]
    public string WorkoutId { get; set; } = string.Empty;
}

/// <summary>
/// Biometrics for PostWorkout payload
/// </summary>
public class Biometrics
{
    [JsonPropertyName("heart_rate_samples")]
    public List<HeartRateSample> HeartRateSamples { get; set; } = new();

    [JsonPropertyName("total_calories")]
    public double TotalCalories { get; set; }
}

/// <summary>
/// Heart rate sample for PostBiometrics
/// </summary>
public class HeartRateSample
{
    [JsonPropertyName("bpm")]
    public double Bpm { get; set; }

    [JsonPropertyName("timestamp_ms")]
    public long TimestampMs { get; set; }
}

/// <summary>
/// Exercise for PostWorkout payload
/// </summary>
public class Exercise
{
    [JsonPropertyName("exercise_template_id")]
    public string ExerciseTemplateId { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("rest_timer_seconds")]
    public int RestTimerSeconds { get; set; }

    [JsonPropertyName("sets")]
    public List<Set> Sets { get; set; } = new();

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("volume_doubling_enabled")]
    public bool VolumeDoublingEnabled { get; set; }

    [JsonPropertyName("superset_id")]
    public int? SupersetId { get; set; }
}

/// <summary>
/// Set for PostExercise
/// </summary>
public class Set
{
    [JsonPropertyName("completed_at")]
    public string CompletedAt { get; set; } = string.Empty;

    [JsonPropertyName("distance_meters")]
    public double DistanceMeters { get; set; }

    [JsonPropertyName("duration_seconds")]
    public int? DurationSeconds { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("weight_kg")]
    public double WeightKg { get; set; }

    [JsonPropertyName("reps")]
    public int? Reps { get; set; }

    [JsonPropertyName("rpe")]
    public double? Rpe { get; set; }
}