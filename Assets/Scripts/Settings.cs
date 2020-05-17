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
    /// Used by <see cref="InputSystem"/> for left and right movement.
    /// </summary>
    public static float InputSenstivity = 0.1f;
}
