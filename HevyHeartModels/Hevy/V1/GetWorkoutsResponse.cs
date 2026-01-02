using System.Text.Json.Serialization;

namespace HevyHeartModels.Hevy.V1;

public class HevyWorkout
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("routine_id")]
    public string RoutineId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("start_time")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("end_time")]
    public DateTime EndTime { get; set; }

    [JsonPropertyName("is_private")]
    public bool IsPrivate { get; set; }

    [JsonPropertyName("exercises")]
    public List<HevyExercise> Exercises { get; set; } = new();
}

public class HevyExercise
{
    [JsonPropertyName("title")]
    public virtual string Title { get; set; } = string.Empty;

    [JsonPropertyName("exercise_template_id")]
    public string ExerciseTemplateId { get; set; } = string.Empty;

    [JsonPropertyName("sets")]
    public List<HevySet> Sets { get; set; } = new();

   
}


public class HevySet
{
    // Public property without JSON serialization
    [JsonPropertyName("index")]
    public virtual int Index { get; set; }

    [JsonPropertyName("type")]
    public virtual string Type { get; set; } = string.Empty;

    [JsonPropertyName("weight_kg")]
    public virtual double? WeightKg { get; set; }

    [JsonPropertyName("reps")]
    public virtual int? Reps { get; set; }

    [JsonPropertyName("distance_meters")]
    public virtual double? DistanceMeters { get; set; }

    [JsonPropertyName("duration_seconds")]
    public virtual int? DurationSeconds { get; set; }

    [JsonPropertyName("completed_at")]
    public virtual string? CompletedAt { get; set; }
}



public class HevyWorkoutsResponse
{
    [JsonPropertyName("workouts")]
    public List<HevyWorkout> Workouts { get; set; } = new();

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("page_count")]
    public int PageCount { get; set; }
}