using UnityEngine;

public class AccelerometerInput : AndroidInput
{
    public override SteeringDirection CurrentSteeringDirection { get { return this.GetHorizontalDirectionBasedOnAxis(); } }

    public AccelerometerInput(ExtendedButton acceleratorPedal, ExtendedButton left, ExtendedButton right, ExtendedButton brake)
        : base(acceleratorPedal, left, right, brake)
    {
        left.gameObject.SetActive(false);
        right.gameObject.SetActive(false);
    }

    public SteeringDirection GetHorizontalDirectionBasedOnAxis()
    {
        var direction = Input.acceleration.x; 
        if (direction > 0.3)
        {
            return SteeringDirection.Right;
        }
        if (direction < -0.3)
        {
            return SteeringDirection.Left;
        }

        return SteeringDirection.Straight;          
    }
}
