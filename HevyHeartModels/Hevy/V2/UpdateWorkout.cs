using System.Text.Json.Serialization;

namespace HevyHeartModels.Hevy.V2;

public class UpdateWorkout
{
    [JsonPropertyName("workoutUpdate")]
    public WorkoutUpdate WorkoutUpdate { get; set; } = new();
}

public class WorkoutUpdate
{
    [JsonPropertyName("exercises")]
    public List<UpdateExercise> Exercises { get; set; } = new();

    [JsonPropertyName("start_and_end_time")]
    public StartAndEndTime StartAndEndTime { get; set; } = new();
}

public class UpdateExercise
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("exercise_template_id")]
    public string ExerciseTemplateId { get; set; } = string.Empty;

    [JsonPropertyName("rest_timer_seconds")]
    public int RestTimerSeconds { get; set; }

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("volume_doubling_enabled")]
    public bool VolumeDoublingEnabled { get; set; }

    [JsonPropertyName("sets")]
    public List<UpdateSet> Sets { get; set; } = new();
}

public class UpdateSet
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("weight_kg")]
    public double? WeightKg { get; set; }

    [JsonPropertyName("reps")]
    public int? Reps { get; set; }

    [JsonPropertyName("rpe")]
    public double? Rpe { get; set; }

    [JsonPropertyName("duration_seconds")]
    public int? DurationSeconds { get; set; }

    [JsonPropertyName("completed_at")]
    public string? CompletedAt { get; set; }
}

public class StartAndEndTime
{
    [JsonPropertyName("start_time")]
    public long StartTime { get; set; }

    [JsonPropertyName("end_time")]
    public long EndTime { get; set; }
}