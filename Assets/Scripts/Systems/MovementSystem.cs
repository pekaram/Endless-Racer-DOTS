using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

public class MovementSystem : JobComponentSystem
{
    [BurstCompile]
    struct MovementJob : IJobForEach<HeroComponent>
    {
        public float DeltaTime;
        public float HorizontalInput;
        public float VerticalInput;

        private const float Speed = 0.1f;

        public void Execute(ref HeroComponent moveSpeed)
        {
            if (VerticalInput > 0)
            {
                moveSpeed.Speed += Speed;
            }
            else if(VerticalInput < 0)
            {
                moveSpeed.Speed -= Speed;
            }
            else if(moveSpeed.Speed > 0)
            {
                moveSpeed.Speed -= Speed;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        MovementJob movementJob = new MovementJob
        {
            DeltaTime = Time.deltaTime,
            HorizontalInput = Input.GetAxis("Horizontal"),
            VerticalInput = Input.GetAxis("Vertical"),
        };

        return movementJob.Schedule(this, inputDeps);
    }
}
