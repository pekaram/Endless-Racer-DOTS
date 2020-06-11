using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using Unity.Physics.Systems;

public class CarMovementSystem : JobComponentSystem
{
    /// <summary>
    /// Reference to the hero, to pull speed data.
    /// </summary>
    private Entity heroEntity;

    [BurstCompile]
    [ExcludeComponent(typeof(HeroComponent))]
    struct MovementJob : IJobForEach<CarComponent, Translation>
    {
        public float deltaTime;

        /// <summary>
        /// Currently hero speed to reflect that on cars.
        /// </summary>
        public float HeroSpeed;
        
        /// <summary>
        /// Translating speed in KM to units to translate on map
        /// </summary>
        public const float KMPerSecondToTranslationUnit = 20;
       
        public void Execute(ref CarComponent carComponent, ref Translation translation)
        {
            if (carComponent.IsCollided)
            {
                carComponent.Speed = 0;
            }

            // TODO: this value should be pulled from a visual gameobject that can easily select that.
            if (translation.Value.z > -9.5f)
            {
                var heroSpeedPerSecond = HeroSpeed * deltaTime;
                var streetCarSpeedPerSecond = carComponent.Speed * deltaTime;
                translation.Value.z += (-heroSpeedPerSecond + streetCarSpeedPerSecond) / KMPerSecondToTranslationUnit;
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

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        MovementJob movementJob = new MovementJob
        {
            HeroSpeed = this.World.EntityManager.GetComponentData<CarComponent>(heroEntity).Speed,
            deltaTime = UnityEngine.Time.deltaTime
        };

        return movementJob.Schedule(this, inputDeps);
    }
}
