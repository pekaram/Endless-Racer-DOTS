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
        /// Archetype used for acessing data from chucnks
        /// </summary>
        public ArchetypeChunkComponentType<Translation> TranslationType;

        /// <summary>
        /// Archetype used for acessing data from chucnks
        /// </summary>
        public ArchetypeChunkComponentType<CarComponent> CarComponentType;

        /// <summary>
        /// Archetype used for acessing data from chucnks
        /// </summary>
        [ReadOnly] public ArchetypeChunkEntityType EntityType;

        /// <summary>
        /// Chunks array
        /// </summary>
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;

        /// <summary>
        /// Random object injected from system for random spawns and speeds.
        /// </summary>
        public long RandomObject;
        
        public void Execute(Entity entity, int index, ref GenerationSlotComponent slotComponent)
        {
            if (this.Time - slotComponent.LastGenerationTimeStamp < this.TimeBetweenBatches)
            {
                return;
            }           

            for(var i = 0; i < Chunks.Length; i++)
            {
                var cars = Chunks[i].GetNativeArray(this.CarComponentType);
                var positions = Chunks[i].GetNativeArray(this.TranslationType);
                for (var j = 0; j < cars.Length; j++)
                {                   
                    var distance = math.distancesq(slotComponent.Position.Value, positions[j].Value);
                    distance = math.abs(slotComponent.Position.Value.z - positions[j].Value.z);
                    // Distance should be enough to ensure two cara don't drop above each other
                    // TODO: for different vechile sizes a more robust solution might be needed.
                    // For now they are the same use first.
                    if (distance < cars[0].CubeColliderSize.z + 1.5f)
                    {
                        slotComponent.IsOccupied = true;
                        // Slot done, continue
                        return;
                    }
                    else
                    {
                        slotComponent.IsOccupied = false;
                    }

                    var random = new Unity.Mathematics.Random((uint)(this.RandomObject));
                    var randomIndex = random.NextInt(0, this.NumberOfGenerationSlots);
                    if (cars[j].IsDisabled && !slotComponent.IsOccupied && index == randomIndex)
                    {
                        var componentData = cars[j];
                        componentData.IsDisabled = false;
                        componentData.IsCollided = false;
                        componentData.Speed = random.NextInt(50, 70);
                        cars[j] = componentData;
                        var shiftedSlotPosition = slotComponent.Position;
                        shiftedSlotPosition.Value.z += this.HeroZ + 12;

                        positions[j] = shiftedSlotPosition;
                        slotComponent.IsOccupied = true;
                        slotComponent.LastGenerationTimeStamp = this.Time;
                        // Slot done, continue
                        return;
                    }
                }
            }
        }
    }
    
    protected override void OnCreate()
    {
        // Query for cars with following components
        EntityQueryDesc carsQuery = new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(CarComponent), typeof(Translation)},
            None = new ComponentType[] { typeof(HeroComponent)}
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

        var translationType = GetArchetypeChunkComponentType<Translation>(false);
        var carComponentType = GetArchetypeChunkComponentType<CarComponent>(false);
        var entityType = GetArchetypeChunkEntityType();
        var chunks = streetCarsGroup.CreateArchetypeChunkArray(Allocator.TempJob);
        SpawnJob spwanJob = new SpawnJob
        {
            Time = Time.ElapsedTime,
            TimeBetweenBatches = 0.2f,
            TranslationType = translationType,
            CarComponentType = carComponentType,
            EntityType = entityType,
            Chunks = chunks,
            RandomObject = DateTime.Now.Ticks,
            NumberOfGenerationSlots = Settings.NumberOfGenerationSlots,
            HeroZ = heroTranslation.Value.z
        };

        return spwanJob.Schedule(this, inputDeps);
    }
}