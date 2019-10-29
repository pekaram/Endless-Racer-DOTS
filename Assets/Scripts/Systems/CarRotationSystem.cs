using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;


public class CarRotationSystem : JobComponentSystem
{
    [BurstCompile]
    struct RotationJob : IJobForEach<CarComponent, Translation, Rotation>
    {        
        public void Execute(ref CarComponent carComponent,ref Translation translation, ref Rotation rotation)
        {
            if(carComponent.Angle != 0)
            {
                translation.Value.x += carComponent.Angle / 1000 ;
            }
        }       
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        RotationJob rotationJob = new RotationJob
        {
        };
        return rotationJob.Schedule(this, inputDeps);
    }
}
