using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

/// <summary>
/// Handles wheel rotation.
/// </summary>
public class WheelRotationSystem : FixedUpdateSystem
{
    [BurstCompile]
    struct WheelRotationJob : IJobForEach<WheelComponent, Rotation>
    {
        public float DeltaTime;

        public void Execute([ReadOnly] ref WheelComponent wheelComponent, ref Rotation rotation)
        {
            // Fixed value for now, update based on car speed.
            rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(new float3(Vector3.right), 10 * DeltaTime));
        }
    }

    protected override JobHandle OnFixedUpdate(JobHandle inputDeps)
    {
        WheelRotationJob rotationJob = new WheelRotationJob
        {
            DeltaTime = Time.deltaTime
        };
        return rotationJob.Schedule(this, inputDeps);
    }
}