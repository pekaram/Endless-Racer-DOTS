using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Photon.Pun;

/// <summary>
/// Handles deterministic traffic by sharing same randomness seed among all clients
/// </summary>
public class InvsibileCarsWorker : WorkerThread
{
    private Translation heroPos;

    private EntityManager entityManager;

    private Entity heroEntity;

    private List<UnrenderedCarComponent> visibleCars = new List<UnrenderedCarComponent>();

    private List<UnrenderedCarComponent> invisibleCars = new List<UnrenderedCarComponent>();

    private System.Random random = new System.Random(2456);

    private const int TrafficCarCount = 2000;

    private float GapBetweenCars = 5;

    private float TrafficCarSpeed = 20;

    private float VisibilityDistance = 70;

    protected override void PreUpdate()
    {
        this.heroPos = this.entityManager.GetComponentData<Translation>(this.heroEntity);

        // Pass visible cars to DOTS' systems
        foreach (var car in this.visibleCars)
        {
            var readyNotSpawn = this.entityManager.CreateEntity();
            this.entityManager.AddComponentData<UnrenderedCarComponent>(readyNotSpawn, car);
        }

        this.visibleCars.Clear();
    }

    protected override void OnThreadUpdate()
    {
        this.MoveUnrenderedCars();
    }

    private float GetLag()
    {
        if (!PhotonNetwork.IsConnected)
        {
            return 0;
        }

        var time = Mathf.Abs(PhotonNetwork.ServerTimestamp - (int)PhotonNetwork.CurrentRoom.CustomProperties["Time"]);
        var lag = (float)System.TimeSpan.FromMilliseconds(time).TotalSeconds;
        return lag;       
    }

    protected override void Initialize()
    {
        this.entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        this.heroEntity = FindObjectOfType<SystemManager>().Hero;

        var lag = this.GetLag();
        for (var i = 1; i < TrafficCarCount + 1; i++)
        {
            invisibleCars.Add(new UnrenderedCarComponent
            {
                translation = new Translation
                {
                    Value = new float3(random.Next(-4, 4), 0, (GapBetweenCars * i) + ((TrafficCarSpeed / Settings.KMToTranslationUnit) * lag))
                }
            });
        }
    }

    private void MoveUnrenderedCars()
    {
        for (var i = 0; i < invisibleCars.Count; i++)
        {
            if (invisibleCars[i].translation.Value.z - this.heroPos.Value.z < VisibilityDistance)
            {
                this.visibleCars.Add(invisibleCars[i]);
                this.invisibleCars.RemoveAt(i);
            }
            else
            {
                var invisibleCar = invisibleCars[i];
                invisibleCar.translation.Value.z += (TrafficCarSpeed / Settings.KMToTranslationUnit) * (this.DeltaTime);
                invisibleCars[i] = invisibleCar;
            }
        }
    }
}
