using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

/// <summary>
/// Not fully implemented, doesn't do much yet.
/// </summary>
public class CarRotationSystem : FixedUpdateSystem
{
    [BurstCompile]
    struct RotationJob : IJobForEach<CarComponent, Translation, Rotation>
    {
        public float DeltaTime;

        public float AllowedHorizontalWidth;

        public void Execute(ref CarComponent carComponent, ref Translation translation, ref Rotation rotation)
        {
            var isInSteeringArea = math.abs(translation.Value.x + carComponent.SteeringIndex) < AllowedHorizontalWidth / 2;
            if (carComponent.SteeringIndex != 0 && isInSteeringArea)
            {
                translation.Value.x += carComponent.SteeringIndex;
            }

            //rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(new float3(1,0,0), 5 * DeltaTime));
        }       
    }

    protected override JobHandle OnFixedUpdate(JobHandle inputDeps)
    {
        RotationJob rotationJob = new RotationJob
        {
            AllowedHorizontalWidth = Settings.RoadWidth,
            DeltaTime = Time.deltaTime
        };

        return rotationJob.Schedule(this, inputDeps);
    }
}