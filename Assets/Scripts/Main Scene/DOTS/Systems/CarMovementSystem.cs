using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using System;

public class CarMovementSystem : JobComponentSystem
{
    /// <summary>
    /// Reference to the hero, to pull speed data.
    /// </summary>
    private Entity heroEntity;

    private int heroID;

    [BurstCompile]
    //[ExcludeComponent(typeof(HeroComponent))]
    struct MovementJob : IJobForEach<CarComponent, Translation>
    {
        /// <summary>
        /// Currently hero speed to reflect that on cars.
        /// </summary>
        public float HeroSpeed;
        
        /// <summary>
        /// Translating speed in KM to units to translate on map
        /// </summary>
        public float KMToTranslationUnit;

        public float DeltaTime;

        public float HeroZ;

        public int HeroId;

        public void Execute(ref CarComponent carComponent, ref Translation translation)
        {
            // TODO: Could be improved 
            if (carComponent.IsCollided)
            {
                carComponent.Speed = 0;
            }

            translation.Value.z += (carComponent.Speed / this.KMToTranslationUnit) * DeltaTime;

            if (carComponent.ID == this.HeroId)
            {
                return;
            }

            if(translation.Value.z - HeroZ < -10)
            {
                carComponent.IsDisabled = true;
            }
        }
    }
    
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        this.heroEntity = this.GetSingletonEntity<HeroComponent>();
        this.heroID = this.EntityManager.GetComponentData<CarComponent>(this.heroEntity).ID;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var heroTranslation = World.EntityManager.GetComponentData<Translation>(this.heroEntity);

        MovementJob movementJob = new MovementJob
        {
            HeroSpeed = this.World.EntityManager.GetComponentData<CarComponent>(heroEntity).Speed,
            DeltaTime = Time.DeltaTime,
            HeroZ = heroTranslation.Value.z,
            HeroId = this.heroID,
            KMToTranslationUnit = Settings.KMToTranslationUnit
        };

        return movementJob.Schedule(this, inputDeps);
    }
}
