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
public class InputSystem : JobComponentSystem
{
    /// <summary>
    /// Injected in <see cref="SystemManager"/>
    /// </summary>
    public IGameInput GameInput { get; set; }

    [BurstCompile]
    [RequireComponentTag(typeof(HeroComponent))]
    struct MovementJob : IJobForEach<CarComponent, Rotation>
    {
        public SteeringDirection CurrentSteeringDirection;

        public MoveDirection CurrentMoveDirection;

        /// <summary>
        /// Speed senstivity.
        /// </summary>
        public float SpeedPedalSenstivity;

        /// <summary>
        /// navigating sensitivity
        /// </summary>
        public float SteeringSenstivity;

        public float TimeDelta;

        public float BrakePedalSenstivity;

        public int MaxSpeed;

        public float IdleSpeedLoss;

        public void Execute(ref CarComponent carComponent, ref Rotation rotation)
        {
            this.HandleVerticalSpeed(ref carComponent, ref rotation);
            this.HandleSteering(ref carComponent, ref rotation);
        }

        public void HandleSteering(ref CarComponent carComponent, ref Rotation rotation)
        {
            if (carComponent.Speed < 0.1)
            {
                this.CurrentSteeringDirection = SteeringDirection.Straight;
            }

            var tiltingAngle = (carComponent.Speed / MaxSpeed) * 0.1f;
            if (this.CurrentSteeringDirection == SteeringDirection.Right)
            {
                carComponent.SteeringIndex = this.SteeringSenstivity;
            }
            else if (this.CurrentSteeringDirection == SteeringDirection.Left)
            {
                carComponent.SteeringIndex = -this.SteeringSenstivity;
            }
            else
            {
                carComponent.SteeringIndex = 0;
            }
        }

        private void HandleVerticalSpeed(ref CarComponent carComponent, ref Rotation rotation)
        {
            var totalSpeed = this.SpeedPedalSenstivity * this.TimeDelta;
            carComponent.IsBraking = false;

            if (this.CurrentMoveDirection == MoveDirection.Forward)
            {
                carComponent.Speed += totalSpeed - (totalSpeed * (carComponent.Speed / MaxSpeed));
                return;
            }

            if (carComponent.Speed <= 0)
            {
                carComponent.Speed = 0;
                return;
            }

            if (this.CurrentMoveDirection == MoveDirection.Backward)
            {
                carComponent.Speed -= BrakePedalSenstivity * this.TimeDelta;
                carComponent.IsBraking = true;
                return;
            }


            carComponent.Speed -= IdleSpeedLoss * this.TimeDelta;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        MovementJob movementJob = new MovementJob
        {
            CurrentSteeringDirection = this.GameInput.CurrentSteeringDirection,
            CurrentMoveDirection = this.GameInput.CurrentMoveDirection,
            SpeedPedalSenstivity = Settings.InputSpeedSenstivity,
            SteeringSenstivity = Settings.SteeringSenstivity,
            TimeDelta = Time.DeltaTime,
            MaxSpeed = Settings.MaxSpeed,
            IdleSpeedLoss = Settings.IdleSpeedLoss,
            BrakePedalSenstivity = Settings.BrakeSenstivity,
        };
        
        Entities
            .ForEach((ref CarComponent carComponent, ref Rotation rotation,
                      in HeroComponent heroComponent) =>
            {
                movementJob.Execute(ref carComponent, ref rotation);
            }).Run();

        return default;
    }
}