using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using System;

/// <summary>
/// Inherit to allow fixed updating in systems, to ensure correct physics.
/// TODO: for frequent updating faster than FPS this won't catch up.
/// Not urgent with the current update rate though, can be improved later on.
/// </summary>
public abstract class FixedUpdateSystem : JobComponentSystem
{
    private DateTime? lastOnUpdateTimeStamp;

    protected TimeSpan timeSinceLastUpdate { get; set; }
    
    [BurstCompile]
    struct BlankJob : IJob
    {
        public void Execute(){}
    }

    protected abstract JobHandle OnFixedUpdate(JobHandle inputDeps);
   
    protected sealed override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (!this.lastOnUpdateTimeStamp.HasValue)
        {
            lastOnUpdateTimeStamp = DateTime.Now;
        }

        this.timeSinceLastUpdate = DateTime.Now - this.lastOnUpdateTimeStamp.Value;
        
        if (this.timeSinceLastUpdate < TimeSpan.FromSeconds(Time.fixedDeltaTime))
        {
            BlankJob waitJob = new BlankJob();
            return waitJob.Schedule(inputDeps);
        }
        else
        {
            this.lastOnUpdateTimeStamp = DateTime.Now;
            return this.OnFixedUpdate(inputDeps);
        }
    }
}