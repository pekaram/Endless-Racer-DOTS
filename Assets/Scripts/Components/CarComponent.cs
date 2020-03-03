using Unity.Entities;
using Unity.Mathematics;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;


[Serializable]
public struct CarComponent : IComponentData
{
    public float Speed;

    public float Angle;

    public Vector3 modelSize;

    public bool IsDisabled;

    public bool IsDestroyed;
}