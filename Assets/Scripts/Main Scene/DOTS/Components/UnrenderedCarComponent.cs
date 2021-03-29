using Unity.Entities;
using System;
using UnityEngine;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;

public struct UnrenderedCarComponent : IComponentData
{
    public Translation translation;
}
