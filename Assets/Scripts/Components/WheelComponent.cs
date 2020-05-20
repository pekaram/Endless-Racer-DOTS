using Unity.Entities;
using System;

/// <summary>
/// A tag for a wheel entity. More info about the wheel can be added here.
/// </summary>
[Serializable]
public struct WheelComponent : IComponentData
{
    public Entity Parent;
}
