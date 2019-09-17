using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;


public struct GenerationSlotComponent : IComponentData
{
    public float LastGenerationTimeStamp;

    public bool IsOccupied;

    public Translation ReadOnlyPosition;
}
