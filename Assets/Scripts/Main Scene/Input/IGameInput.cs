using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SteeringDirection
{
    Straight,
    Left,
    Right
}

public enum MoveDirection
{
    Idle,
    Forward,
    Backward
}

public interface IGameInput 
{
    SteeringDirection CurrentSteeringDirection { get; }

    MoveDirection CurrentMoveDirection { get; }
}