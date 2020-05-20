using Unity.Entities;
using System;
using UnityEngine;

[Serializable]
public struct CarComponent : IComponentData
{
    /// <summary>
    /// Indentification id that always stays on this car 
    /// </summary>
    public Guid ID;

    /// <summary>
    /// Car speed, subtracted from player's speed for reflecting current player speed
    /// </summary>
    public float Speed;

    /// <summary>
    /// Represents the current steering angle of the wheel, 0 means no rotation.
    /// </summary>
    public float SteeringIndex;

    /// <summary>
    /// Data for the box collider surrounding this object. 
    /// Feels less accurate than <see cref="CapsuleColliderData"/> but this can be re-visted
    /// </summary>
    public Vector3 CubeColliderSize;

    /// <summary>
    /// Is disabled is for cars that are out of player's sight
    /// </summary>
    public bool IsDisabled;

    /// <summary>
    /// For cars that did hit other object on the road.
    /// </summary>
    public bool IsCollided;

    /// <summary>
    /// Data for the capsule collider surrounding this car.
    /// </summary>
    public CapsuleColliderData CapsuleColliderData;
}