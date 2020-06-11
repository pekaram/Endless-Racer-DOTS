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

        public float MaxSteeringPerSecond;

        public void Execute(ref CarComponent carComponent, ref Translation translation, ref Rotation rotation)
        {
            var translationValue = carComponent.SteeringIndex * (MaxSteeringPerSecond * DeltaTime);
            var isInSteeringArea = math.abs(translationValue + translation.Value.x) < AllowedHorizontalWidth / 2;
            if (carComponent.SteeringIndex != 0 && isInSteeringArea)
            {
                translation.Value.x += translationValue;
            }

            //rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.AxisAngle(new float3(1,0,0), 5 * DeltaTime));
        }       
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        RotationJob rotationJob = new RotationJob
        {
            AllowedHorizontalWidth = Settings.RoadWidth,
            DeltaTime = Time.deltaTime,
            MaxSteeringPerSecond = Settings.MaxHoriznontalMovePerSecond
        };

        return rotationJob.Schedule(this, inputDeps);
    }
}