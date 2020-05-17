using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class CollisionSystem : JobComponentSystem
{
    private BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

    private EntityQuery streetCarsGroup;

    // [BurstCompile]
    // EntityCommandBuffer isn't supported in burst yet, enable when Unity adds support.
    struct CollisionJob2 : IJobForEachWithEntity<CarComponent, Translation>
    {
        public EntityCommandBuffer.Concurrent EntityCommandBuffer;
        
        /// <summary>
        /// ArcheTypes for querying all cars.
        /// </summary>
        [ReadOnly] public ArchetypeChunkComponentType<CarComponent> CarComponentType;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;
        [ReadOnly] public ArchetypeChunkEntityType EntityType;

        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;

        public void Execute(Entity entity, int index, [ReadOnly] ref CarComponent carComponent, [ReadOnly] ref Translation translation)
        {
            for (var i = 0; i < Chunks.Length; i++)
            {
                var cars = Chunks[i].GetNativeArray(this.CarComponentType);
                var positions = Chunks[i].GetNativeArray(this.TranslationType);
                var entities = Chunks[i].GetNativeArray(this.EntityType);
                for (var j = 0; j < cars.Length; j++)
                {
                    // Skip if same exact car.
                    if(cars[j].ID == carComponent.ID)
                    {
                        continue;
                    }

                    // Collision already, not need to re-enter this code
                    if(cars[j].IsCollided && carComponent.IsCollided)
                    {
                        continue;
                    }
                    
                    var xDelta = Mathf.Abs(translation.Value.x - positions[j].Value.x);
                    var zDelta = Mathf.Abs(translation.Value.z - positions[j].Value.z);
                    if (this.IsCapsuleCollision(xDelta, zDelta, carComponent))
                    {
                        // If burst whines about this log message, remove it.
                        Debug.Log("Collison for: " + cars[j].ID + " & " + carComponent.ID);

                        var chunkCarComponent = cars[j];
                        chunkCarComponent.IsCollided = true;
                        this.EntityCommandBuffer.SetComponent(index, entities[j], chunkCarComponent);

                        carComponent.IsCollided = true;
                        this.EntityCommandBuffer.SetComponent(index, entity, carComponent);
                    }
                }
            }
        }

        /// <summary>
        /// Checks against <see cref="CarComponent.CubeColliderSize"/>
        /// </summary>
        /// <param name="xDelta"> difference between both objects on X-axis </param>
        /// <param name="zDelta"> difference between both objects on Z-axis </param>
        /// <param name="carComponent"> of the car in question </param>
        /// <returns> true on collision </returns>
        private bool IsBoxCollision(float xDelta, float zDelta, CarComponent carComponent)
        {
            return xDelta < carComponent.CubeColliderSize.x && zDelta < carComponent.CubeColliderSize.z;
        }

        /// <summary>
        /// Checks against <see cref="CarComponent.CapsuleColliderData"/>
        /// </summary>
        /// <param name="xDelta"> difference between both objects on X-axis </param>
        /// <param name="zDelta"> difference between both objects on Z-axis </param>
        /// <param name="carComponent"> of the car in question </param>
        /// <returns> true on collision </returns>
        private bool IsCapsuleCollision(float xDelta, float zDelta, CarComponent carComponent)
        {
            return xDelta < (carComponent.CapsuleColliderData.Radius * 2) && zDelta < carComponent.CapsuleColliderData.Height;
        }
    }

    protected override void OnCreate()
    {
        this.entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
        EntityQueryDesc carsQuery = new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(CarComponent), typeof(Translation) },
        };

        // Get the ComponentGroup
        this.streetCarsGroup = GetEntityQuery(carsQuery);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var translationType = GetArchetypeChunkComponentType<Translation>(true);
        var carComponentType = GetArchetypeChunkComponentType<CarComponent>(true);
        var entityType = GetArchetypeChunkEntityType();
        var chunks = streetCarsGroup.CreateArchetypeChunkArray(Allocator.TempJob, out var handle);

        CollisionJob2 collisionJob = new CollisionJob2
        {
            EntityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            CarComponentType = carComponentType,
            EntityType = entityType,
            Chunks = chunks,
            TranslationType = translationType
        };


        return collisionJob.Schedule(this, JobHandle.CombineDependencies(handle, inputDeps));
    }
}
