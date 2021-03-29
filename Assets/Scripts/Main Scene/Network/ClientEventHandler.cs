using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;
using Unity.Entities;
using Unity.Transforms;

public struct InputPress
{
    public int ServerTimeStamp;
    public MoveDirection MoveDirection;
    public float Speed;
    public float Z;
}

public struct SteeringPress
{
    public int ServerTimestamp;
    public SteeringDirection SteeringDirection;
    public float X;
}

// TODO: class has unrelated duties 
public class ClientEventHandler : MonoBehaviour, IOnEventCallback
{
    [SerializeField]
    private GameObject testTarget;
    private SteeringDirection previousEnemySteering = SteeringDirection.Straight;
    private SteeringDirection currentEnemySteering = SteeringDirection.Straight;
    private float enemyCarX = 0;

    [SerializeField]
    private SystemManager systemManager;

    private InputSnapshots inputSnapshots;
    
    private MoveDirection lastSentDirection = MoveDirection.Idle;

    private SteeringDirection lastSentSteertingDirection = SteeringDirection.Straight;

    private Queue<InputPress> receivedForwardInputs = new Queue<InputPress>();

    private Queue<SteeringPress> recievedSteeringInputs = new Queue<SteeringPress>();

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            this.enabled = false;
            Debug.LogWarning("not connected");
            return;
        }

        this.inputSnapshots = this.systemManager.InputSnapshots;
    }

    private void Update()
    {
        var data = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<CarComponent>(this.systemManager.Hero);
        
        this.SendVerticalInputOnChange();

        this.SendHorizontalInputOnChange();

        this.SyncVertical();

        this.SyncHorionztal();
    }
    
    public void SendHorizontalInputOnChange()
    {
        var translation = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<Translation>(this.systemManager.Hero);
        var data = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<CarComponent>(this.systemManager.Hero);

        if (inputSnapshots.SideInputs.Count == 0)
        {
            return;
        }

        var input = inputSnapshots.SideInputs.Dequeue();

        if (input.SteeringDirection == this.lastSentSteertingDirection)
        {
            return;
        }

        object[] content = new object[] { input.SteeringDirection, input.ServerTimestamp, translation.Value.x }; 
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(2, content, raiseEventOptions, SendOptions.SendReliable);

        this.lastSentSteertingDirection = input.SteeringDirection;
    }

    public void SendVerticalInputOnChange()
    {
        var translation = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<Translation>(this.systemManager.Hero);
        var data = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<CarComponent>(this.systemManager.Hero);
        
        if (inputSnapshots.ForwardInputs.Count == 0)
        {            
            return;
        }

        var input = inputSnapshots.ForwardInputs.Dequeue();

        if(input.MoveDirection == this.lastSentDirection)
        {
            return;
        }

        object[] content = new object[] {input.MoveDirection, input.ServerTimeStamp, data.Speed, translation.Value.z };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(1, content, raiseEventOptions, SendOptions.SendReliable);

        this.lastSentDirection = input.MoveDirection;
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == 1)
        {
            object[] data = (object[])photonEvent.CustomData;
            this.enemyCarDirection = (MoveDirection)data[0];
            var timestamp = (int)data[1];
            var unsyncedSpeed = (float)data[2];
            var enemyCarZ = (float)data[3];

            receivedForwardInputs.Enqueue(new InputPress { MoveDirection = this.enemyCarDirection, ServerTimeStamp = timestamp, Speed = unsyncedSpeed, Z = enemyCarZ });           
        }

        if (eventCode == 2)
        {
            object[] data = (object[])photonEvent.CustomData;
            var steering = (SteeringDirection)data[0];
            var steeringLag = (int)data[1];
            var X = (float)data[2];

           this.recievedSteeringInputs.Enqueue(new SteeringPress { ServerTimestamp = steeringLag, SteeringDirection = steering, X = X });
        }
    }
    
    private void SyncHorionztal()
    {
        var input = default(SteeringPress);
        if (this.recievedSteeringInputs.Count > 0)
        {
            input = this.recievedSteeringInputs.Dequeue();
            this.currentEnemySteering = input.SteeringDirection;
        }
        
        //TODO: Switch
        if (this.currentEnemySteering == SteeringDirection.Left)
        {
            this.enemyCarX -= Settings.SteeringSenstivity * Time.deltaTime * 20;
            this.previousEnemySteering = SteeringDirection.Left;
        }

        else if (this.currentEnemySteering == SteeringDirection.Right)
        {
            this.enemyCarX += Settings.SteeringSenstivity * Time.deltaTime * 20;
            this.previousEnemySteering = SteeringDirection.Right;
        }

        else if (this.currentEnemySteering == SteeringDirection.Straight && this.previousEnemySteering != SteeringDirection.Straight)
        {
            this.enemyCarX = input.X;
            this.previousEnemySteering = SteeringDirection.Straight;
        }

        this.testTarget.transform.position = new Vector3(this.enemyCarX, this.testTarget.transform.position.y, this.testTarget.transform.position.z);
    }

    public MoveDirection enemyCarDirection = MoveDirection.Idle;

    private MoveDirection previousDirection = MoveDirection.Idle;

    public float enemyCarSpeed = 0;

    //TODO: Ugly big method
    private void SyncVertical()
    {
        var enemySpeed = 0f;
        var enemyZ = 0f;
        var serverTimestamp = 0;
        if (this.receivedForwardInputs.Count > 0)
        {
            var firstInput = this.receivedForwardInputs.Dequeue();
            this.enemyCarDirection = firstInput.MoveDirection;

            serverTimestamp = firstInput.ServerTimeStamp;
            enemySpeed = firstInput.Speed;
            enemyZ = firstInput.Z;
        }
        
        var totalSpeed = (Settings.InputSpeedSenstivity) * Time.deltaTime;
        var totalLagSeconds = (float)System.TimeSpan.FromMilliseconds(Mathf.Abs(PhotonNetwork.ServerTimestamp - serverTimestamp)).TotalSeconds;
        var totalLaggedSpeed = Settings.InputSpeedSenstivity * totalLagSeconds;

        if (this.enemyCarDirection == MoveDirection.Forward)
        {        
            if (this.previousDirection != MoveDirection.Forward)
            {
                this.enemyCarSpeed = enemySpeed + totalLaggedSpeed; 
                this.previousDirection = MoveDirection.Forward;

                enemyZ += (totalLagSeconds) * enemySpeed / Settings.KMToTranslationUnit;
                // TODO: perhaps smooth this on smaller values
                this.testTarget.transform.position = new Vector3(this.testTarget.transform.position.x, this.testTarget.transform.position.y, enemyZ);
                return;
            }

            this.enemyCarSpeed += totalSpeed;
        }
        else if (this.enemyCarDirection == MoveDirection.Backward)
        {
            if (enemyCarSpeed < 0)
            {
                this.enemyCarSpeed = 0;
                this.enemyCarDirection = MoveDirection.Idle;
                this.previousDirection = MoveDirection.Idle;
                return;
            }

            if (this.previousDirection != MoveDirection.Backward) 
            {
                // Compensate for lag
                this.enemyCarSpeed = enemySpeed - (Settings.BrakeSenstivity * totalLagSeconds);
                this.enemyCarSpeed += Settings.BrakeSenstivity * Time.deltaTime;
                this.previousDirection = MoveDirection.Backward;
            }
            
            this.enemyCarSpeed -= Settings.BrakeSenstivity * Time.deltaTime;
        }
        //Idle 
        else if (this.enemyCarSpeed > 0)
        {
            if(this.previousDirection != MoveDirection.Idle)
            {
                // Compensate for lag 
                this.enemyCarSpeed = enemySpeed - (Settings.IdleSpeedLoss * totalLagSeconds);
                this.enemyCarSpeed += Settings.IdleSpeedLoss * Time.deltaTime;
                this.previousDirection = MoveDirection.Idle;
            }

            this.enemyCarSpeed -= Settings.IdleSpeedLoss * Time.deltaTime;
        }
        
        if (enemyCarSpeed < 0)
        {
            enemyCarSpeed = 0;
            return;
        }
        this.testTarget.transform.Translate(0, 0, (this.enemyCarSpeed / Settings.KMToTranslationUnit) * Time.deltaTime);
    }    
}