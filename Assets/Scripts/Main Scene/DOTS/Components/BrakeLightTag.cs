using Unity.Entities;

[GenerateAuthoringComponent]
public struct BrakeLight : IComponentData
{
    public bool IsOn;
}
