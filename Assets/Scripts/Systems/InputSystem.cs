using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

/// <summary>
/// Input handling for <see cref="HeroComponent"/>'s car
/// </summary>
public class InputSystem : FixedUpdateSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(HeroComponent))]
    struct MovementJob : IJobForEach<CarComponent, Rotation>
    {
        /// <summary>
        /// <see cref="Input.GetAxis(string)"/> injected from the system
        /// </summary>
        public float HorizontalInput;

        /// <summary>
        /// <see cref="Input.GetAxis(string)"/> injected from the system
        /// </summary>
        public float VerticalInput;

        /// <summary>
        /// Left and right senstivity.
        /// </summary>
        public float Senstivity;

        public void Execute(ref CarComponent carComponent, ref Rotation rotation)
        {
            this.HandleVerticalSpeed(ref carComponent);
            if (HorizontalInput > 0)
            {
                carComponent.Angle = HorizontalInput * 45;
            }
            else if (HorizontalInput < 0)
            {
                carComponent.Angle = HorizontalInput * 45;
            }
            else
            {
                carComponent.Angle = 0;
            }

            // Rotation is currently not implemented yet
            //rotation.Value = rotation.Value * Quaternion.Euler(Vector3.up * 0.1f);
        }

        private void HandleVerticalSpeed(ref CarComponent moveSpeed)
        {
            if (VerticalInput > 0)
            {
                moveSpeed.Speed += Senstivity;
            }
            else if (VerticalInput < 0)
            {
                moveSpeed.Speed -= Senstivity;
            }
            else if (moveSpeed.Speed > 0)
            {
                moveSpeed.Speed -= Senstivity;
            }
        }
    }
  
    protected override JobHandle OnFixedUpdate(JobHandle inputDeps)
    {
        MovementJob movementJob = new MovementJob
        {
            HorizontalInput = Input.GetAxis("Horizontal"),
            VerticalInput = Input.GetAxis("Vertical"),
            Senstivity = Settings.InputSenstivity
        };

        return movementJob.Schedule(this, inputDeps);
    }
}
