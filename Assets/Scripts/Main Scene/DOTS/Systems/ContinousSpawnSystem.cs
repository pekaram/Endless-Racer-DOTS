using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using System;

/// <summary>
/// Responsible for reseting cars that are <see cref="CarComponent.IsDisabled"/> 
/// </summary>
public class ContinousSpawnSystem : JobComponentSystem
{
    private BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

    /// <summary>
    /// Quering all street cars. Hero Excluded
    /// </summary>
    private EntityQuery streetCarsGroup;

    /// <summary>
    /// Reference to the hero, to pull speed data.
    /// </summary>
    private Entity heroEntity;


    [BurstCompile]
    struct SpawnJob : IJobForEachWithEntity<GenerationSlotComponent>
    {
        [ReadOnly] public int ActiveCars;

        [ReadOnly] public ComponentDataFromEntity<CarComponent> CarDataFromEntity;

        public EntityCommandBuffer.Concurrent EntityCommandBuffer;

        public float HeroZ;

        /// <summary>
        /// <see cref="Time.unscaledDeltaTime"/> unjected from the system.
        /// </summary>
        public double Time;

        /// <summary>
        /// Delta between generation time
        /// </summary>
        public float TimeBetweenBatches;

        /// <summary>
        /// Number of generation slots, injected from <see cref="Settings.NumberOfGenerationSlots"/>
        /// </summary>
        public int NumberOfGenerationSlots;
        
        /// <summary>
        /// Random object injected from system for random spawns and speeds.
        /// </summary>
        public long RandomObject;

        public float SpawningDistance;

        public int MinSpeed;

        public int MaxSpeed;

        [ReadOnly] public DynamicBuffer<LinkedEntityGroup> CarModels;
        
        public void Execute(Entity entity, int index, ref GenerationSlotComponent slotComponent)
        {
            if (ActiveCars > 6)
            {
                return;
            }
            
            if (this.Time - slotComponent.LastGenerationTimeStamp < this.TimeBetweenBatches)
            {
                slotComponent.IsOccupied = true;

                return;
            }
            else
            {
                slotComponent.IsOccupied = false;
            }
            

            var random = new Unity.Mathematics.Random((uint)RandomObject);
            var slot = random.NextInt(0, 5);
            var randomIndex = random.NextInt(0, this.NumberOfGenerationSlots);

            if(slot != index)
            {
                return;
            }

            this.GenerateCar(this.ActiveCars, ref slotComponent, index, random);
        }

        public void GenerateCar(int ActiveCarCount, ref GenerationSlotComponent slotComponent, int index, Unity.Mathematics.Random random)
        {
            var component = this.CarDataFromEntity[this.CarModels[0].Value];
            // With 7 active cars the probability for a collision is almost impossible
            component.ID = random.NextInt(0, int.MaxValue);
            component.Speed = random.NextInt(MinSpeed, MaxSpeed);
            component.IsCollided = false;
            var shiftedSlotPosition = slotComponent.Position;
            shiftedSlotPosition.Value.z += this.HeroZ + this.SpawningDistance;

            slotComponent.IsOccupied = true;
            slotComponent.LastGenerationTimeStamp = this.Time;

            var carModelByIndex = random.NextInt(0, this.CarModels.Length);
            var newCar = this.EntityCommandBuffer.Instantiate(index, this.CarModels[carModelByIndex].Value);

            this.EntityCommandBuffer.RemoveComponent<Disabled>(index, newCar);
            this.EntityCommandBuffer.SetComponent<Translation>(index, newCar, shiftedSlotPosition);

            this.EntityCommandBuffer.AddComponent<CarComponent>(index, newCar, component);
        }
    }


    protected override void OnCreate()
    {
        this.entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();

        // Query for cars with following components
        EntityQueryDesc carsQuery = new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(CarComponent), typeof(Translation) },
            None = new ComponentType[] { typeof(HeroComponent) }
        };

        // Get the ComponentGroup
        streetCarsGroup = GetEntityQuery(carsQuery);
    }


    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        this.heroEntity = this.GetSingletonEntity<HeroComponent>();
    }


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var heroTranslation = World.EntityManager.GetComponentData<Translation>(this.heroEntity);
        
        var carCatalogueParent = this.GetSingletonEntity<StreetCarPrefabsBuffer>();
        var streetCarModels = this.EntityManager.GetBuffer<LinkedEntityGroup>(carCatalogueParent);
        var carDataFromEntity = GetComponentDataFromEntity<CarComponent>();

        var activeCars = streetCarsGroup.CalculateEntityCount();

        SpawnJob spwanJob = new SpawnJob
        {
            Time = Time.ElapsedTime,
            TimeBetweenBatches = 0.7f,
            RandomObject = DateTime.Now.Ticks,
            NumberOfGenerationSlots = Settings.NumberOfGenerationSlots,
            HeroZ = heroTranslation.Value.z,
            CarModels = streetCarModels,
            EntityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            CarDataFromEntity = carDataFromEntity,
            ActiveCars = activeCars,
            SpawningDistance = Settings.SpawningDistanceAheadOfHero,
            MaxSpeed = Settings.StreetCarMaxSpawnSpeed,
            MinSpeed = Settings.StreetCarMinSpawnSpeed
        };

        return spwanJob.Schedule(this, inputDeps);        
    }
}