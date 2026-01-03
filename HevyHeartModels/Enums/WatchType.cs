namespace HevyHeartModels.Enums;

/// <summary>
/// Represents the type of device/watch used to record workout biometrics.
/// </summary>
public enum WatchType
{
    /// <summary>
    /// No watch was used - manual entry or other source
    /// </summary>
    None = 0,
    
    /// <summary>
    /// Apple Watch was used to record biometrics
    /// </summary>
    AppleWatch = 1,
    
    /// <summary>
    /// WearOS watch was used to record biometrics
    /// </summary>
    WearOS = 2
}
