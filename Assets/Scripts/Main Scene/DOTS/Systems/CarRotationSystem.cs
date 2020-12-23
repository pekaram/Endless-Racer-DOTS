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
public class CarRotationSystem : JobComponentSystem
{
    [BurstCompile]
    struct RotationJob : IJobForEach<CarComponent, Translation, Rotation>
    {
        public float DeltaTime;

        public float AllowedHorizontalWidth;

        public void Execute(ref CarComponent carComponent, ref Translation translation, ref Rotation rotation)
        {
            var translationValue = carComponent.SteeringIndex * (20 * DeltaTime);
            var isInSteeringArea = math.abs(translationValue + translation.Value.x) < AllowedHorizontalWidth / 2;
            if (carComponent.SteeringIndex != 0 && isInSteeringArea)
            {
                translation.Value.x += translationValue;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        RotationJob rotationJob = new RotationJob
        {
            AllowedHorizontalWidth = Settings.RoadWidth,
            DeltaTime = Time.DeltaTime
        };

        return rotationJob.Schedule(this, inputDeps);
    }
}