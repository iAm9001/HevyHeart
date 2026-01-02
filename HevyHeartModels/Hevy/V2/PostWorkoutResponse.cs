using System.Text.Json.Serialization;

namespace HevyHeartModels.Hevy.V2;

/// <summary>
/// Response model for Hevy V2 API create workout endpoint.
/// Returned after successfully creating a new workout, containing the complete workout details including generated IDs and biometrics.
/// </summary>
public class PostWorkoutResponse
{
    /// <summary>
    /// Gets or sets the unique identifier for the newly created workout.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the short identifier used for sharing or quick lookup of the workout.
    /// </summary>
    [JsonPropertyName("short_id")]
    public string ShortId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sequential index or workout number for the user.
    /// Used for tracking workout progression over time.
    /// </summary>
    [JsonPropertyName("index")]
    public long Index { get; set; }

    /// <summary>
    /// Gets or sets the name or title of the workout.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description or notes for the workout.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

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

    /// <summary>
    /// Gets or sets the timestamp when the workout was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the workout was last updated.
    /// </summary>
    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the routine this workout is based on, if applicable.
    /// Empty string if the workout is not part of a routine.
    /// </summary>
    [JsonPropertyName("routine_id")]
    public string RoutineId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this workout was recorded using an Apple Watch.
    /// </summary>
    [JsonPropertyName("apple_watch")]
    public bool AppleWatch { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this workout was recorded using a WearOS watch.
    /// </summary>
    [JsonPropertyName("wearos_watch")]
    public bool WearosWatch { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who owns this workout.
    /// </summary>
    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the username of the workout owner.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of the workout owner's profile image.
    /// </summary>
    [JsonPropertyName("profile_image")]
    public string ProfileImage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the user account is verified.
    /// </summary>
    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether warmup sets are included in volume calculations.
    /// </summary>
    [JsonPropertyName("include_warmup_sets")]
    public bool IncludeWarmupSets { get; set; }

    /// <summary>
    /// Gets or sets the sequential workout number for this user.
    /// Indicates this is the user's Nth workout (e.g., 100th workout).
    /// </summary>
    [JsonPropertyName("nth_workout")]
    public int NthWorkout { get; set; }

    /// <summary>
    /// Gets or sets the total number of likes this workout has received from other users.
    /// </summary>
    [JsonPropertyName("like_count")]
    public int LikeCount { get; set; }

    /// <summary>
    /// Gets or sets the list of profile image URLs for users who liked this workout.
    /// Used for displaying like previews.
    /// </summary>
    [JsonPropertyName("like_images")]
    public List<string> LikeImages { get; set; } = new();

    /// <summary>
    /// Gets or sets a preview list of users who liked this workout.
    /// Typically shows the first few likes for display purposes.
    /// </summary>
    [JsonPropertyName("preview_workout_likes")]
    public List<PostResponsePreviewWorkoutLike> PreviewWorkoutLikes { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the workout is marked as private.
    /// Private workouts are not visible to other users or followers.
    /// </summary>
    [JsonPropertyName("is_private")]
    public bool IsPrivate { get; set; }

    /// <summary>
    /// Gets or sets the biometrics data for this workout, including heart rate and calories.
    /// </summary>
    [JsonPropertyName("biometrics")]
    public PostResponseBiometrics Biometrics { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether biometrics data (heart rate, calories) is publicly visible.
    /// If false, biometrics are only visible to the workout owner.
    /// </summary>
    [JsonPropertyName("is_biometrics_public")]
    public bool IsBiometricsPublic { get; set; }

    /// <summary>
    /// Gets or sets the list of comments made on this workout.
    /// Each item's structure contains comment details from other users.
    /// </summary>
    [JsonPropertyName("comments")]
    public List<object> Comments { get; set; } = new();

    /// <summary>
    /// Gets or sets the total number of comments on this workout.
    /// </summary>
    [JsonPropertyName("comment_count")]
    public int CommentCount { get; set; }

    /// <summary>
    /// Gets or sets the list of media attachments (photos, videos) associated with the workout.
    /// Each item's structure depends on the media type.
    /// </summary>
    [JsonPropertyName("media")]
    public List<object> Media { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of URLs for images attached to the workout.
    /// </summary>
    [JsonPropertyName("image_urls")]
    public List<string> ImageUrls { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of exercises performed in this workout.
    /// Each exercise contains sets, notes, and exercise-specific metadata.
    /// </summary>
    [JsonPropertyName("exercises")]
    public List<PostResponseExercise> Exercises { get; set; } = new();

    /// <summary>
    /// Gets or sets the estimated total volume in kilograms for this workout.
    /// Calculated as the sum of (weight × reps) across all sets.
    /// </summary>
    [JsonPropertyName("estimated_volume_kg")]
    public double EstimatedVolumeKg { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the current authenticated user has liked this workout.
    /// </summary>
    [JsonPropertyName("is_liked_by_user")]
    public bool IsLikedByUser { get; set; }
}

/// <summary>
/// Contains biometrics data for a workout in the post workout response.
/// Includes heart rate samples, total calories, and calculated metrics.
/// </summary>
public class PostResponseBiometrics
{
    /// <summary>
    /// Gets or sets the total calories burned during the workout.
    /// </summary>
    [JsonPropertyName("total_calories")]
    public int TotalCalories { get; set; }

    /// <summary>
    /// Gets or sets the list of heart rate samples recorded during the workout.
    /// Each sample contains a heart rate measurement and timestamp.
    /// </summary>
    [JsonPropertyName("heart_rate_samples")]
    public List<PostResponseHeartRateSample> HeartRateSamples { get; set; } = new();

    /// <summary>
    /// Gets or sets the average heart rate in beats per minute (bpm) for the entire workout.
    /// Calculated from the heart rate samples.
    /// </summary>
    [JsonPropertyName("average_heart_rate")]
    public int AverageHeartRate { get; set; }
}

/// <summary>
/// Represents a single heart rate measurement at a specific point in time during a workout.
/// </summary>
public class PostResponseHeartRateSample
{
    /// <summary>
    /// Gets or sets the heart rate measurement in beats per minute (bpm).
    /// </summary>
    [JsonPropertyName("bpm")]
    public int Bpm { get; set; }

    /// <summary>
    /// Gets or sets the Unix timestamp in milliseconds when this heart rate sample was recorded.
    /// </summary>
    [JsonPropertyName("timestamp_ms")]
    public long TimestampMs { get; set; }
}

/// <summary>
/// Represents an exercise in a post workout response.
/// Contains detailed exercise information including localized titles, muscle groups, and all sets performed.
/// </summary>
public class PostResponseExercise
{
    /// <summary>
    /// Gets or sets the unique identifier for this exercise instance in the workout.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name or title of the exercise in English (e.g., "Bench Press", "Squat").
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Spanish (Español) localized title for the exercise.
    /// Null if translation is not available.
    /// </summary>
    [JsonPropertyName("es_title")]
    public string? EsTitle { get; set; }

    /// <summary>
    /// Gets or sets the German (Deutsch) localized title for the exercise.
    /// Null if translation is not available.
    /// </summary>
    [JsonPropertyName("de_title")]
    public string? DeTitle { get; set; }

    /// <summary>
    /// Gets or sets the French (Français) localized title for the exercise.
    /// Null if translation is not available.
    /// </summary>
    [JsonPropertyName("fr_title")]
    public string? FrTitle { get; set; }

    /// <summary>
    /// Gets or sets the Italian (Italiano) localized title for the exercise.
    /// Null if translation is not available.
    /// </summary>
    [JsonPropertyName("it_title")]
    public string? ItTitle { get; set; }

    /// <summary>
    /// Gets or sets the Portuguese (Português) localized title for the exercise.
    /// Null if translation is not available.
    /// </summary>
    [JsonPropertyName("pt_title")]
    public string? PtTitle { get; set; }

    /// <summary>
    /// Gets or sets the Korean (???) localized title for the exercise.
    /// Null if translation is not available.
    /// </summary>
    [JsonPropertyName("ko_title")]
    public string? KoTitle { get; set; }

    /// <summary>
    /// Gets or sets the Japanese (???) localized title for the exercise.
    /// Null if translation is not available.
    /// </summary>
    [JsonPropertyName("ja_title")]
    public string? JaTitle { get; set; }

    /// <summary>
    /// Gets or sets the Turkish (Türkçe) localized title for the exercise.
    /// Null if translation is not available.
    /// </summary>
    [JsonPropertyName("tr_title")]
    public string? TrTitle { get; set; }

    /// <summary>
    /// Gets or sets the Russian (???????) localized title for the exercise.
    /// Null if translation is not available.
    /// </summary>
    [JsonPropertyName("ru_title")]
    public string? RuTitle { get; set; }

    /// <summary>
    /// Gets or sets the Simplified Chinese (????) localized title for the exercise.
    /// Null if translation is not available.
    /// </summary>
    [JsonPropertyName("zh_cn_title")]
    public string? ZhCnTitle { get; set; }

    /// <summary>
    /// Gets or sets the Traditional Chinese (????) localized title for the exercise.
    /// Null if translation is not available.
    /// </summary>
    [JsonPropertyName("zh_tw_title")]
    public string? ZhTwTitle { get; set; }

    /// <summary>
    /// Gets or sets the superset identifier if this exercise is part of a superset.
    /// Null if the exercise is not part of a superset.
    /// Exercises with the same superset ID are performed together as a superset.
    /// </summary>
    [JsonPropertyName("superset_id")]
    public int? SupersetId { get; set; }

    /// <summary>
    /// Gets or sets the recommended rest time in seconds between sets for this exercise.
    /// </summary>
    [JsonPropertyName("rest_seconds")]
    public int RestSeconds { get; set; }

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
    /// Gets or sets the type of exercise (e.g., "strength", "cardio", "flexibility").
    /// </summary>
    [JsonPropertyName("exercise_type")]
    public string ExerciseType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category of equipment required for this exercise (e.g., "Barbell", "Dumbbell", "Machine", "Bodyweight").
    /// </summary>
    [JsonPropertyName("equipment_category")]
    public string EquipmentCategory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL to a custom exercise image uploaded by the user.
    /// Null if using the default exercise image or if no custom image was uploaded.
    /// </summary>
    [JsonPropertyName("custom_exercise_image_url")]
    public string? CustomExerciseImageUrl { get; set; }

    /// <summary>
    /// Gets or sets the URL to a thumbnail image for this exercise.
    /// Null if no thumbnail is available.
    /// </summary>
    [JsonPropertyName("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Gets or sets the primary muscle group targeted by this exercise (e.g., "Chest", "Legs", "Back").
    /// </summary>
    [JsonPropertyName("muscle_group")]
    public string MuscleGroup { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of secondary muscle groups worked by this exercise.
    /// </summary>
    [JsonPropertyName("other_muscles")]
    public List<string> OtherMuscles { get; set; } = new();

    /// <summary>
    /// Gets or sets the priority or order of this exercise within the workout.
    /// Lower numbers indicate higher priority or earlier execution order.
    /// </summary>
    [JsonPropertyName("priority")]
    public int Priority { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether volume doubling is enabled for this exercise.
    /// When enabled, each rep counts as double volume (typically for exercises performed on each side).
    /// </summary>
    [JsonPropertyName("volume_doubling_enabled")]
    public bool VolumeDoublingEnabled { get; set; }

    /// <summary>
    /// Gets or sets the list of sets performed for this exercise.
    /// Each set contains weight, reps, duration, or other performance metrics.
    /// </summary>
    [JsonPropertyName("sets")]
    public List<PostResponseSet> Sets { get; set; } = new();
}

/// <summary>
/// Represents a single set within an exercise in a post workout response.
/// Contains performance data, personal records, and completion information.
/// </summary>
public class PostResponseSet
{
    /// <summary>
    /// Gets or sets the unique identifier for this set instance.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the zero-based index indicating the order of this set within the exercise.
    /// </summary>
    [JsonPropertyName("index")]
    public int Index { get; set; }

    /// <summary>
    /// Gets or sets a visual indicator for the set (e.g., "W" for warmup, "F" for failure, "D" for dropset).
    /// Empty string for normal sets.
    /// </summary>
    [JsonPropertyName("indicator")]
    public string Indicator { get; set; } = string.Empty;

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
    /// Gets or sets the distance covered in this set in meters.
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
    /// Gets or sets a custom metric value for this set, if applicable.
    /// The structure of this object depends on the exercise template configuration.
    /// Null if no custom metric is defined for this set.
    /// </summary>
    [JsonPropertyName("custom_metric")]
    public object? CustomMetric { get; set; }

    /// <summary>
    /// Gets or sets the Rate of Perceived Exertion (RPE) for this set.
    /// Scale typically ranges from 1-10, where 10 is maximum effort.
    /// Null if RPE was not recorded for this set.
    /// </summary>
    [JsonPropertyName("rpe")]
    public double? Rpe { get; set; }

    /// <summary>
    /// Gets or sets the list of personal records (PRs) achieved in this set.
    /// Deprecated: Use <see cref="PersonalRecords"/> instead for consistency with API naming.
    /// </summary>
    [JsonPropertyName("prs")]
    public List<PostResponsePr> Prs { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of personal records (PRs) achieved in this set.
    /// Each PR indicates a new personal best for a specific metric (weight, reps, volume, etc.).
    /// </summary>
    [JsonPropertyName("personalRecords")]
    public List<PostResponsePr> PersonalRecords { get; set; } = new();

    /// <summary>
    /// Gets or sets the timestamp when this set was completed.
    /// </summary>
    [JsonPropertyName("completed_at")]
    public string CompletedAt { get; set; } = string.Empty;
}

/// <summary>
/// Represents a personal record (PR) achievement in a workout set.
/// Indicates a new personal best for a specific performance metric.
/// </summary>
public class PostResponsePr
{
    /// <summary>
    /// Gets or sets the type of personal record (e.g., "weight", "reps", "volume", "one_rep_max").
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the personal record achievement.
    /// The unit depends on the type (e.g., kg for weight, count for reps).
    /// </summary>
    [JsonPropertyName("value")]
    public double Value { get; set; }
}

/// <summary>
/// Represents a preview of a user who liked a workout in the post workout response.
/// Used for displaying a quick preview of workout likes in the UI.
/// </summary>
public class PostResponsePreviewWorkoutLike
{
    /// <summary>
    /// Gets or sets the username of the user who liked the workout.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL to the user's profile picture.
    /// </summary>
    [JsonPropertyName("profile_pic")]
    public string ProfilePic { get; set; } = string.Empty;
}