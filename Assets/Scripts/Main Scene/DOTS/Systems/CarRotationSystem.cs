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

        public float MaxSpeed;

        public void Execute(ref CarComponent carComponent, ref Translation translation, ref Rotation rotation)
        {
            var translationValue = carComponent.SteeringIndex * (20 * DeltaTime);
            var isInSteeringArea = math.abs(translationValue + translation.Value.x) < AllowedHorizontalWidth / 2;
            if (carComponent.SteeringIndex != 0 && isInSteeringArea)
            {
                translation.Value.x += translationValue;
            }

            this.HandleSteeringVisuals(ref carComponent, ref rotation);
        }

        private void HandleSteeringVisuals(ref CarComponent carComponent, ref Rotation rotation)
        {
            var steeringMangitude = 0;
            if (carComponent.SteeringIndex < 0)
            {
                steeringMangitude = -1;
            }
            else if (carComponent.SteeringIndex > 0)
            {
                steeringMangitude = 1;
            }
            var forwardTilt = carComponent.IsBraking ? 1 : 0;

            var tiltingAngle = (carComponent.Speed / MaxSpeed) * 0.1f;
            rotation.Value =
                  Quaternion.AngleAxis(3 * steeringMangitude, Vector3.up) *
                  Quaternion.AngleAxis(tiltingAngle * steeringMangitude, Vector3.forward) *
                  Quaternion.AngleAxis(0.5f * forwardTilt, Vector3.right);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        RotationJob rotationJob = new RotationJob
        {
            AllowedHorizontalWidth = Settings.RoadWidth,
            DeltaTime = Time.DeltaTime,
            MaxSpeed = Settings.MaxSpeed           
        };

        return rotationJob.Schedule(this, inputDeps);
    }
}