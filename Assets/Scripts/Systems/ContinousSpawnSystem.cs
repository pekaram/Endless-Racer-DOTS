using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using System;

public class ContinousSpawnSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

    EntityQuery streetCarsGroup;

    [BurstCompile]
    struct SpawnJob : IJobForEachWithEntity<GenerationSlotComponent>
    {
        public float Time;

        public float TimeBetweenBatches;

        [ReadOnly] public EntityCommandBuffer.Concurrent CommandBuffer;

        public ArchetypeChunkComponentType<Translation> TranslationType;

        public ArchetypeChunkComponentType<CarComponent> CarComponentType;

        public long randomObject;

        [ReadOnly] public ArchetypeChunkEntityType EntityType;

        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;

        public void Execute(Entity entity, int index, ref GenerationSlotComponent slotComponent)
        {
            if(Time - slotComponent.LastGenerationTimeStamp < TimeBetweenBatches)
            {
                return;
            }           

            for(var i = 0; i < Chunks.Length; i++)
            {
                var cars = Chunks[i].GetNativeArray(this.CarComponentType);
                var positions = Chunks[i].GetNativeArray(this.TranslationType);
                for (var j = 0; j < cars.Length; j++)
                {
                    if (slotComponent.IsOccupied)
                    {
                        var distance = math.distancesq(slotComponent.ReadOnlyPosition.Value, positions[j].Value);
                        if (distance < 0.1)
                        {
                            slotComponent.IsOccupied = true;
                            return;
                        }
                        else
                        {
                            slotComponent.IsOccupied = false;
                        }
                    }

                    if (cars[j].IsDisabled && !slotComponent.IsOccupied)
                    {
                        var componentData = cars[j];
                        componentData.IsDisabled = false;
                        var random = new Unity.Mathematics.Random((uint)(randomObject));
                        componentData.Speed = random.NextInt(5, 100);
                        cars[j] = componentData;
                        positions[j] = slotComponent.ReadOnlyPosition;
                        slotComponent.IsOccupied = true;
                        slotComponent.LastGenerationTimeStamp = Time;
                        return;
                    }
                }
            }
        }
    }
    
    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();

        // Query for ScoreBoxes with following components
        EntityQueryDesc scoreBoxQuery = new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(CarComponent), typeof(Translation)},
        };

        // Get the ComponentGroup
        streetCarsGroup = GetEntityQuery(scoreBoxQuery);
    }


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var translationType = GetArchetypeChunkComponentType<Translation>(false);
        var carComponentType = GetArchetypeChunkComponentType<CarComponent>(false);
        var entityType = GetArchetypeChunkEntityType();
        var chunks = streetCarsGroup.CreateArchetypeChunkArray(Allocator.TempJob, out var handle);
        
        SpawnJob spwanJob = new SpawnJob
        {
            CommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            Time = Time.unscaledTime,
            TimeBetweenBatches = 0.2f,
            TranslationType = translationType,
            CarComponentType = carComponentType,
            EntityType = entityType,
            Chunks = chunks,
            randomObject = DateTime.Now.Ticks
    };

        return spwanJob.Schedule(this, JobHandle.CombineDependencies(handle, inputDeps));
    }
}
