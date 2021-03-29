using System.Collections.Generic;

public class InputSnapshots
{
    public Queue<InputPress> ForwardInputs = new Queue<InputPress>();

    public Queue<SteeringPress> SideInputs = new Queue<SteeringPress>();

    public void Add(MoveDirection moveDirection, SteeringDirection steeringDirection)
    {
        this.ForwardInputs.Enqueue(new InputPress { MoveDirection = moveDirection, ServerTimeStamp = Photon.Pun.PhotonNetwork.ServerTimestamp });
        this.SideInputs.Enqueue(new SteeringPress { SteeringDirection = steeringDirection, ServerTimestamp = Photon.Pun.PhotonNetwork.ServerTimestamp });
    }   
}
