using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class InputSystem : JobComponentSystem
{
    [BurstCompile]
    struct MovementJob : IJobForEach<HeroComponent, CarComponent>
    {
        public float HorizontalInput;
        public float VerticalInput;

        private const float Speed = 0.1f;

        public void Execute([ReadOnly] ref HeroComponent moveSpeed, ref CarComponent carComponent)
        {
            this.HandleVerticalSpeed(ref carComponent);
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
