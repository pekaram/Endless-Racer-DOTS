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
    public static float InputSpeedSenstivity = 0.1f;

    /// <summary>
    /// Used by <see cref="InputSystem"/> for navigating senstivity
    /// </summary>
    public static float SteeringSenstivity = 0.1f;

    /// <summary>
    /// Total allowed play width. Currently car won't steer further
    /// </summary>
    public static float RoadWidth = 9;
}
