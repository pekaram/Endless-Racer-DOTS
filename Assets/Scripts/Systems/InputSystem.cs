using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;


public class InputSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(HeroComponent))]
    struct MovementJob : IJobForEach<CarComponent, Rotation>
    {
        public float HorizontalInput;
        public float VerticalInput;

        private const float Speed = 0.1f;

        public void Execute(ref CarComponent carComponent, ref Rotation rotation)
        {
            this.HandleVerticalSpeed(ref carComponent);
            if(HorizontalInput > 0)
            {
                carComponent.Angle = HorizontalInput * 45;
            }
            else if(HorizontalInput < 0)
            {
                carComponent.Angle = HorizontalInput * 45;
            }
            else
            {
                carComponent.Angle = 0;
            }

            //rotation.Value = rotation.Value * Quaternion.Euler(Vector3.up * 0.1f);
        }

        private void HandleVerticalSpeed(ref CarComponent moveSpeed)
        {
            if (VerticalInput > 0)
            {
                moveSpeed.Speed += Speed;
            }
            else if (VerticalInput < 0)
            {
                moveSpeed.Speed -= Speed;
            }
            else if (moveSpeed.Speed > 0)
            {
                moveSpeed.Speed -= Speed;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        MovementJob movementJob = new MovementJob
        {
            HorizontalInput = Input.GetAxis("Horizontal"),
            VerticalInput = Input.GetAxis("Vertical"),
        };

        return movementJob.Schedule(this, inputDeps);
    }
}
