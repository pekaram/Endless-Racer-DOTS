using Unity.Entities;
using System;

/// <summary>
/// A tag for a wheel entity. More info about the wheel can be added here.
/// </summary>
[Serializable]
[GenerateAuthoringComponent]
public struct WheelComponent : IComponentData
{
    public int ParentID;

    public Entity Parent;

    public float Speed;
}
