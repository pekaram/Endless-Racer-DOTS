using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidInput : IGameInput
{
    public AndroidInput(ExtendedButton acceleratorPedal, ExtendedButton left, ExtendedButton right, ExtendedButton brake)
    {
        acceleratorPedal.gameObject.SetActive(true);
        left.gameObject.SetActive(true);
        right.gameObject.SetActive(true);

        acceleratorPedal.OnButtonUp += this.OnAcceleratorPedalButtonUp;
        acceleratorPedal.OnButtonDown += this.OnAcceleratorPedalButtonDown;

        left.OnButtonDown += this.OnLeftButtonDown;
        left.OnButtonUp += this.OnLeftButtonUp;

        right.OnButtonDown += this.OnRightButtonDown;
        right.OnButtonUp += this.OnRightButtonUp;

        brake.OnButtonDown += OnBrakeDown;
        brake.OnButtonUp += OnBrakeUp;

        this.CurrentMoveDirection = MoveDirection.Idle;
        this.CurrentSteeringDirection = SteeringDirection.Straight;
    }

    public virtual SteeringDirection CurrentSteeringDirection { get; set; }

    public MoveDirection CurrentMoveDirection { get; set; }


    private void OnBrakeUp()
    {
        this.CurrentMoveDirection = MoveDirection.Idle;
    }

    private void OnBrakeDown()
    {
        this.CurrentMoveDirection = MoveDirection.Backward;
    }

    private void OnRightButtonUp()
    {
        this.CurrentSteeringDirection = SteeringDirection.Straight;
    }

    private void OnRightButtonDown()
    {
        this.CurrentSteeringDirection = SteeringDirection.Right;
    }

    private void OnLeftButtonUp()
    {
        this.CurrentSteeringDirection = SteeringDirection.Straight;
    }

    private void OnLeftButtonDown()
    {
        this.CurrentSteeringDirection = SteeringDirection.Left;
    }

    private void OnAcceleratorPedalButtonDown()
    {
        this.CurrentMoveDirection = MoveDirection.Forward;
    }

    private void OnAcceleratorPedalButtonUp()
    {
        this.CurrentMoveDirection = MoveDirection.Idle;
    }
}