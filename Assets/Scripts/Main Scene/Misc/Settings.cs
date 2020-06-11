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
    public static float InputSpeedSenstivityPerSecond = 20;

    public static float InputBrakeSenstivityPerSecond = 18;

    /// <summary>
    /// Speed lost when car is idle with no brake or acceleration
    /// </summary>
    public static float IdleSpeedLostPerSecond = 8f;

    /// <summary>
    /// acceleration will keep getting working the closer the car is reaching this speed
    /// </summary>
    public static float MaxCarSpeed = 200;

    /// <summary>
    /// Used by <see cref="InputSystem"/> for navigating senstivity
    /// </summary>
    public static float MaxSteeringSenstivity = 1f;

    public static float MaxHoriznontalMovePerSecond = 4;

    /// <summary>
    /// Total allowed play width. Currently car won't steer further
    /// </summary>
    public static float RoadWidth = 9;
}
