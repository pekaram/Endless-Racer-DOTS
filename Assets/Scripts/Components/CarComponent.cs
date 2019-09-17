﻿using Unity.Entities;
using Unity.Mathematics;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;


[Serializable]
public struct CarComponent : IComponentData
{
    public float Speed;

    public bool IsDisabled;
}