using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class SlotComponent : IComponentData
{
    public Translation Position;

    public int Speed;
}
