using Unity.Entities;
using Unity.Mathematics;
using System;
using System.Collections.Generic;
using Unity.Collections;

/*
     * Component data for a Player entity.
    */
[Serializable]
public struct HeroComponent : IComponentData
{
    public float Speed;
}