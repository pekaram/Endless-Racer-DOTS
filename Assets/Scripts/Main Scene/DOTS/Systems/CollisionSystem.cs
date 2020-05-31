using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System;

public class CollisionSystem : JobComponentSystem
{
    private BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

    private EntityQuery streetCarsGroup;

    // [BurstCompile]
    // EntityCommandBuffer isn't supported in burst yet, enable when Unity adds support.
    struct CollisionJob : IJobForEachWithEntity<CarComponent, Translation>
    {
        public EntityCommandBuffer.Concurrent EntityCommandBuffer;
        
        /// <summary>
        /// ArcheTypes for querying all cars.
        /// </summary>
        [ReadOnly] public ArchetypeChunkComponentType<CarComponent> CarComponentType;
        [ReadOnly] public ArchetypeChunkComponentType<Translation> TranslationType;
        [ReadOnly] public ArchetypeChunkEntityType EntityType;

        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> Chunks;

        public CarComponent Hero;
        
        public void Execute(Entity entity, int index,[ReadOnly] ref CarComponent carComponent, [ReadOnly] ref Translation translation)
        {
            for (var i = 0; i < Chunks.Length; i++)
            {
                var cars = Chunks[i].GetNativeArray(this.CarComponentType);
                var positions = Chunks[i].GetNativeArray(this.TranslationType);
                var entities = Chunks[i].GetNativeArray(this.EntityType);
                for (var j = 0; j < cars.Length; j++)
                {

                    var isAlreadyCollision = cars[j].IsCollided && carComponent.IsCollided;
                    var isSameCar = cars[j].ID == carComponent.ID;               
                    // Skip if same exact car or Collision already, not need to re-enter this code
                    if (isAlreadyCollision || isSameCar)
                    {
                        continue;
                    }

                    var xDelta = Mathf.Abs(translation.Value.x - positions[j].Value.x);
                    var zDelta = Mathf.Abs(translation.Value.z - positions[j].Value.z);
                    if (this.IsCapsuleCollision(xDelta, zDelta, carComponent))
                    {
                        this.OnCollision(index, entity, carComponent, entities[j], cars[j]);

                        continue;
                    }

                    //  close call only from hero perspective
                    if (carComponent.ID != Hero.ID || carComponent.CarInCloseCall != Guid.Empty)
                    {
                        continue;
                    }

                    var isInCloseCall = this.IsCloseCall(xDelta, zDelta, carComponent);
                    if (isInCloseCall)
                    {
                        this.OnCloseCall(index, entity, carComponent, entities[j], cars[j]);
                        
                        continue;
                    }
                    
                    if (carComponent.CarInCloseCall == cars[j].ID)
                    {
                        this.OnCloseEnded(index, entity, carComponent, entities[j], cars[j]);
                    }
                }
            }
        }

        private void OnCloseEnded(int jobIndex, Entity firstEntity, CarComponent firstCar, Entity secondEntity, CarComponent secondCar)
        {
            firstCar.CarInCloseCall = Guid.Empty;
            this.EntityCommandBuffer.SetComponent(jobIndex, firstEntity, firstCar);

            secondCar.CarInCloseCall = Guid.Empty;
            this.EntityCommandBuffer.SetComponent(jobIndex, secondEntity, secondCar);
        }

        private void OnCloseCall(int jobIndex, Entity firstEntity, CarComponent firstCar, Entity secondEntity, CarComponent secondCar)
        {
            var chunkCarComponent = secondCar;
            chunkCarComponent.CarInCloseCall = firstCar.ID;
            this.EntityCommandBuffer.SetComponent(jobIndex, secondEntity, chunkCarComponent);

            firstCar.CarInCloseCall = secondCar.ID;
            this.EntityCommandBuffer.SetComponent(jobIndex, firstEntity, firstCar);
        }

        private void OnCollision(int jobIndex, Entity firstEntity, CarComponent firstCar, Entity secondEntity, CarComponent secondCar)
        {
            // If burst whines about this log message, remove it.
            Debug.Log("Collision for: " + firstCar.ID + " & " + secondCar.ID);

            secondCar.IsCollided = true;
            this.EntityCommandBuffer.SetComponent(jobIndex, secondEntity, secondCar);

            firstCar.IsCollided = true;
            this.EntityCommandBuffer.SetComponent(jobIndex, firstEntity, firstCar);
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

        private bool IsCloseCall(float xDelta, float zDelta, CarComponent carComponent)
        {
            return xDelta < (carComponent.CapsuleColliderData.Radius * 2) + 1 && zDelta < carComponent.CapsuleColliderData.Height + 1;
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
        var hero = this.GetSingletonEntity<HeroComponent>();
        var heroCarComponent = World.EntityManager.GetComponentData<CarComponent>(hero);
        var chunks = streetCarsGroup.CreateArchetypeChunkArray(Allocator.TempJob, out var handle);
        CollisionJob collisionJob = new CollisionJob
        {
            EntityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            CarComponentType = carComponentType,
            EntityType = entityType,
            Chunks = chunks,
            TranslationType = translationType,
            Hero = heroCarComponent
        };

        var inputdeps = collisionJob.Schedule(this, JobHandle.CombineDependencies(handle, inputDeps));
        entityCommandBufferSystem.AddJobHandleForProducer(inputdeps);
        return inputdeps;
    }
}
