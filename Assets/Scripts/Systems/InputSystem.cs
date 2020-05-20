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
        /// Speed senstivity.
        /// </summary>
        public float SpeedPedalSenstivity;

        /// <summary>
        /// navigating sensitivity
        /// </summary>
        public float SteeringSenstivity;

        public void Execute(ref CarComponent carComponent, ref Rotation rotation)
        {
            this.HandleVerticalSpeed(ref carComponent);
            this.HandleSteering(ref carComponent);
        }

        public void HandleSteering(ref CarComponent carComponent)
        {
            if (HorizontalInput != 0)
            {
                carComponent.SteeringIndex = HorizontalInput * SteeringSenstivity;
            }
            else
            {
                carComponent.SteeringIndex = 0;
            }

            // Rotation is currently not implemented yet
            //rotation.Value = rotation.Value * Quaternion.Euler(Vector3.up * 0.1f);
        }

        private void HandleVerticalSpeed(ref CarComponent carComponent)
        {
            if (VerticalInput > 0)
            {
                carComponent.Speed += SpeedPedalSenstivity;
            }
            else if (VerticalInput < 0)
            {
                carComponent.Speed -= SpeedPedalSenstivity;
            }
            else if (carComponent.Speed > 0)
            {
                carComponent.Speed -= SpeedPedalSenstivity;
            }
        }
    }
  
    protected override JobHandle OnFixedUpdate(JobHandle inputDeps)
    {
        MovementJob movementJob = new MovementJob
        {
            HorizontalInput = Input.GetAxis("Horizontal"),
            VerticalInput = Input.GetAxis("Vertical"),
            SpeedPedalSenstivity = Settings.InputSpeedSenstivity,
            SteeringSenstivity = Settings.SteeringSenstivity
        };

        return movementJob.Schedule(this, inputDeps);
    }
}
