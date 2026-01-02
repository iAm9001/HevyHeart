using System.Text.Json.Serialization;

namespace HevyHeartModels.Hevy.V2;

/// <summary>
/// Response model for HevyHeart V2 API get workout endpoint
/// </summary>
public class GetWorkoutResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("index")]
    public long Index { get; set; }

    [JsonPropertyName("media")]
    public List<object> Media { get; set; } = new();

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("comments")]
    public List<object> Comments { get; set; } = new();

    [JsonPropertyName("end_time")]
    public long EndTime { get; set; }

    [JsonPropertyName("short_id")]
    public string ShortId { get; set; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    [JsonPropertyName("exercises")]
    public List<GetExercise> Exercises { get; set; } = new();

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("image_urls")]
    public List<string> ImageUrls { get; set; } = new();

    [JsonPropertyName("is_private")]
    public bool IsPrivate { get; set; }

    [JsonPropertyName("like_count")]
    public int LikeCount { get; set; }

    [JsonPropertyName("routine_id")]
    public string RoutineId { get; set; } = string.Empty;

    [JsonPropertyName("start_time")]
    public long StartTime { get; set; }

    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;

    [JsonPropertyName("apple_watch")]
    public bool AppleWatch { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("like_images")]
    public List<string> LikeImages { get; set; } = new();

    [JsonPropertyName("nth_workout")]
    public int NthWorkout { get; set; }

    [JsonPropertyName("wearos_watch")]
    public bool WearosWatch { get; set; }

    [JsonPropertyName("comment_count")]
    public int CommentCount { get; set; }

    [JsonPropertyName("profile_image")]
    public string ProfileImage { get; set; } = string.Empty;

    [JsonPropertyName("estimated_volume_kg")]
    public double EstimatedVolumeKg { get; set; }

    [JsonPropertyName("include_warmup_sets")]
    public bool IncludeWarmupSets { get; set; }

    [JsonPropertyName("is_biometrics_public")]
    public bool IsBiometricsPublic { get; set; }

    [JsonPropertyName("preview_workout_likes")]
    public List<GetPreviewWorkoutLike> PreviewWorkoutLikes { get; set; } = new();

    [JsonPropertyName("is_liked_by_user")]
    public bool IsLikedByUser { get; set; }
}

/// <summary>
/// Exercise model for GetWorkoutResponse
/// </summary>
public class GetExercise
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("sets")]
    public List<GetSet> Sets { get; set; } = new();

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("de_title")]
    public string? DeTitle { get; set; }

    [JsonPropertyName("es_title")]
    public string? EsTitle { get; set; }

    [JsonPropertyName("fr_title")]
    public string? FrTitle { get; set; }

    [JsonPropertyName("it_title")]
    public string? ItTitle { get; set; }

    [JsonPropertyName("ja_title")]
    public string? JaTitle { get; set; }

    [JsonPropertyName("ko_title")]
    public string? KoTitle { get; set; }

    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    [JsonPropertyName("pt_title")]
    public string? PtTitle { get; set; }

    [JsonPropertyName("ru_title")]
    public string? RuTitle { get; set; }

    [JsonPropertyName("tr_title")]
    public string? TrTitle { get; set; }

    [JsonPropertyName("media_type")]
    public string? MediaType { get; set; }

    [JsonPropertyName("superset_id")]
    public string? SupersetId { get; set; }

    [JsonPropertyName("zh_cn_title")]
    public string? ZhCnTitle { get; set; }

    [JsonPropertyName("zh_tw_title")]
    public string? ZhTwTitle { get; set; }

    [JsonPropertyName("muscle_group")]
    public string MuscleGroup { get; set; } = string.Empty;

    [JsonPropertyName("rest_seconds")]
    public int RestSeconds { get; set; }

    [JsonPropertyName("exercise_type")]
    public string ExerciseType { get; set; } = string.Empty;

    [JsonPropertyName("other_muscles")]
    public List<string> OtherMuscles { get; set; } = new();

    [JsonPropertyName("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    [JsonPropertyName("equipment_category")]
    public string EquipmentCategory { get; set; } = string.Empty;

    [JsonPropertyName("exercise_template_id")]
    public string ExerciseTemplateId { get; set; } = string.Empty;

    [JsonPropertyName("volume_doubling_enabled")]
    public bool VolumeDoublingEnabled { get; set; }

    [JsonPropertyName("custom_exercise_image_url")]
    public string? CustomExerciseImageUrl { get; set; }
}

/// <summary>
/// Set model for GetWorkoutResponse
/// </summary>
public class GetSet
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("prs")]
    public List<GetPr> Prs { get; set; } = new();

    [JsonPropertyName("rpe")]
    public double? Rpe { get; set; }

    [JsonPropertyName("reps")]
    public int? Reps { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("indicator")]
    public string Indicator { get; set; } = string.Empty;

    [JsonPropertyName("weight_kg")]
    public double? WeightKg { get; set; }

    [JsonPropertyName("completed_at")]
    public string CompletedAt { get; set; } = string.Empty;

    [JsonPropertyName("custom_metric")]
    public object? CustomMetric { get; set; }

    [JsonPropertyName("distance_meters")]
    public double? DistanceMeters { get; set; }

    [JsonPropertyName("personalRecords")]
    public List<GetPr> PersonalRecords { get; set; } = new();

    [JsonPropertyName("duration_seconds")]
    public int? DurationSeconds { get; set; }
}

/// <summary>
/// PR model for GetWorkoutResponse
/// </summary>
public class GetPr
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public double Value { get; set; }
}

/// <summary>
/// PreviewWorkoutLike model for GetWorkoutResponse
/// </summary>
public class GetPreviewWorkoutLike
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("profile_pic")]
    public string ProfilePic { get; set; } = string.Empty;
}