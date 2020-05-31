using Unity.Entities;
using Unity.Transforms;

/// <summary>
/// A slot used for reseting cars 
/// </summary>
public struct GenerationSlotComponent : IComponentData
{
    /// <summary>
    /// Last time this slot got a reset
    /// </summary>
    public float LastGenerationTimeStamp;

    /// <summary>
    /// If this slot is currently hosting a car.
    /// </summary>
    public bool IsOccupied;

    /// <summary>
    /// This slot's position
    /// </summary>
    public Translation Position;
}
