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
/// </summary>
public abstract class FixedUpdateSystem : JobComponentSystem
{
    private DateTime? lastOnUpdateTimeStamp;

    [BurstCompile]
    struct BlankJob : IJob
    {
        public void Execute(){}
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (!this.lastOnUpdateTimeStamp.HasValue)
        {
            lastOnUpdateTimeStamp = DateTime.Now;
        }

        if (DateTime.Now - lastOnUpdateTimeStamp < TimeSpan.FromSeconds(Time.fixedDeltaTime))
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

    protected abstract JobHandle OnFixedUpdate(JobHandle inputDeps);  
}
