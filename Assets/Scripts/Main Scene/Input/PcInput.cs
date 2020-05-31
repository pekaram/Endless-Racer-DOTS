using UnityEngine;

public class PcInput : IGameInput
{
    public SteeringDirection CurrentSteeringDirection { get { return this.GetHorizontalDirectionBasedOnAxis(); } }
    public MoveDirection CurrentMoveDirection { get { return this.GetVerticalDirectionBasedOnAxis(); } }

    public SteeringDirection GetHorizontalDirectionBasedOnAxis()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            return SteeringDirection.Left;
        }
        if(Input.GetKey(KeyCode.RightArrow))
        {
            return SteeringDirection.Right;
        }

        return SteeringDirection.Straight;          
    }

    public MoveDirection GetVerticalDirectionBasedOnAxis()
    {
        var VerticalInput = Input.GetAxis("Vertical");
        if (VerticalInput < 0)
        {
            return MoveDirection.Backward;
        }
        if (VerticalInput > 0)
        {
            return MoveDirection.Forward;
        }

        return MoveDirection.Idle;
    }
}
