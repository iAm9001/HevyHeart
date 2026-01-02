using System.Text.Json.Serialization;

namespace HevyHeartModels.Hevy.V1;

/// <summary>
/// Response model for HevyHeart V1 API get workout endpoint
/// </summary>
public class GetWorkoutResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("routine_id")]
    public string RoutineId { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("start_time")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("end_time")]
    public DateTime EndTime { get; set; }

    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("exercises")]
    public List<V1Exercise> Exercises { get; set; } = new();
}

/// <summary>
/// Exercise model for V1 GetWorkoutResponse
/// </summary>
public class V1Exercise
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("exercise_template_id")]
    public string ExerciseTemplateId { get; set; } = string.Empty;

    [JsonPropertyName("superset_id")]
    public string? SupersetId { get; set; }

    [JsonPropertyName("sets")]
    public List<V1Set> Sets { get; set; } = new();
}

/// <summary>
/// Set model for V1 GetWorkoutResponse
/// </summary>
public class V1Set
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("weight_kg")]
    public double? WeightKg { get; set; }

    [JsonPropertyName("reps")]
    public int? Reps { get; set; }

    [JsonPropertyName("distance_meters")]
    public double? DistanceMeters { get; set; }

    [JsonPropertyName("duration_seconds")]
    public int? DurationSeconds { get; set; }

    [JsonPropertyName("rpe")]
    public double? Rpe { get; set; }

    [JsonPropertyName("custom_metric")]
    public object? CustomMetric { get; set; }
}

