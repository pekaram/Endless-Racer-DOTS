using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using System;

public class CarMovementSystem : FixedUpdateSystem
{
    /// <summary>
    /// Reference to the hero, to pull speed data.
    /// </summary>
    private Entity heroEntity;

    [BurstCompile]
    [ExcludeComponent(typeof(HeroComponent))]
    struct MovementJob : IJobForEach<CarComponent, Translation>
    {
        /// <summary>
        /// Currently hero speed to reflect that on cars.
        /// </summary>
        public float HeroSpeed;
        
        /// <summary>
        /// Translating speed in KM to units to translate on map
        /// </summary>
        public const float KMToTranslationUnit = 1000;

        public float DeltaTime;
        
        public void Execute(ref CarComponent carComponent, ref Translation translation)
        {
            if (carComponent.IsCollided)
            {
                carComponent.Speed = 0;
            }

            // TODO: this value should be pulled from a visual gameobject that can easily select that.
            if (translation.Value.z > -9.5f)
            {
                 translation.Value.z -= (((HeroSpeed - carComponent.Speed)- HeroSpeed/150) / 60) * DeltaTime;
                //translation.Value.z -= ((HeroSpeed - carComponent.Speed) / MovementJob.KMToTranslationUnit) * DeltaTime;
            }
            else
            {
                carComponent.IsDisabled = true;
            }
        }
    }
    
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        this.heroEntity = this.GetSingletonEntity<HeroComponent>();
    }

    protected override JobHandle OnFixedUpdate(JobHandle inputDeps)
    {
        MovementJob movementJob = new MovementJob
        {
            HeroSpeed = this.World.EntityManager.GetComponentData<CarComponent>(heroEntity).Speed,
            DeltaTime = (float)this.timeSinceLastUpdate.TotalSeconds,
        };

        return movementJob.Schedule(this, inputDeps);
    }
}
