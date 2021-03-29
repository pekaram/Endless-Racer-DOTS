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
    /// Reference to the hero, to pull speed data.
    /// </summary>
    private Entity heroEntity;

    [BurstCompile]
    struct SpawnJob : IJobForEachWithEntity<UnrenderedCarComponent>
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
        
        public int CarCount;

        public void Execute(Entity entity, int index, ref UnrenderedCarComponent unrenderedCar)
        {
            var rand = new Unity.Mathematics.Random((uint)RandomObject);
            this.GenerateCar(index, 5, 1, 33, unrenderedCar);
            this.EntityCommandBuffer.DestroyEntity(index, entity);
        }

        public void GenerateCar(int index,int ActiveCarCount, int carModel, int id, UnrenderedCarComponent unrenderedCar)
        {
            var component = this.CarDataFromEntity[this.CarModels[carModel].Value];

            component.ID = id;
            component.Speed = 20;
            component.IsCollided = false;
            var shiftedSlotPosition = unrenderedCar;
            
            var newCar = this.EntityCommandBuffer.Instantiate(index, this.CarModels[carModel].Value);
            this.EntityCommandBuffer.RemoveComponent<Disabled>(index, newCar);
            this.EntityCommandBuffer.SetComponent<Translation>(index, newCar, shiftedSlotPosition.translation);
            //
            this.EntityCommandBuffer.AddComponent<CarComponent>(index, newCar, component);
        }
    }


    protected override void OnCreate()
    {
        this.entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();        
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
            //ActiveCars = activeCars,
            SpawningDistance = Settings.SpawningDistanceAheadOfHero,
            MaxSpeed = Settings.StreetCarMaxSpawnSpeed,
            MinSpeed = Settings.StreetCarMinSpawnSpeed,
            CarCount = Settings.TrafficCount,
        };

        return spwanJob.Schedule(this, inputDeps);
    }
}