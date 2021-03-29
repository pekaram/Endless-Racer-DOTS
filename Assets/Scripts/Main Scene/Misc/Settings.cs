/// <summary>
/// Game settings, that would probably be accesible from the game menu. TODO: see if we want to turn this into a singleton or so.
/// </summary>
public class Settings 
{
    /// <summary>
    /// Used by <see cref="ContinousSpawnSystem"/>
    /// </summary>
    public static int NumberOfGenerationSlots = 5;

    /// <summary>
    /// Used by <see cref="InputSystem"/> for speed.
    /// </summary>
    public static float InputSpeedSenstivity = 7.5f;

    /// <summary>
    /// Used by <see cref="InputSystem"/> for navigating senstivity
    /// </summary>
    public static float SteeringSenstivity = 0.150f;

    /// <summary>
    /// Total allowed play width. Currently car won't steer further
    /// </summary>
    public static float RoadWidth = 8f;

    public static float KMToTranslationUnit = 5;

    public static int MaxSpeed = 150;

    public static float BrakeSenstivity = 70;

    public static float IdleSpeedLoss = 5;

    public static float FollowCameraTurn = 5;

    public static float SpawningDistanceAheadOfHero = 12;

    public static int StreetCarMinSpawnSpeed = 30;

    public static int StreetCarMaxSpawnSpeed = 70;

    public static int TotalRoadDistanceInKM = 230;

    public static int TrafficCount = 8;
}
