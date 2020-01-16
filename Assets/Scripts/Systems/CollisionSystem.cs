using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using System;

public class CollisionSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

    [ExcludeComponent(typeof(HeroComponent))]
    [BurstCompile]
    struct CollisionJob : IJobForEachWithEntity<CarComponent, Translation>
    {
        public Translation HeroTranslation;

        public Entity heroEntity;

        public EntityCommandBuffer.Concurrent EntityCommandBuffer;

        public void Execute(Entity entity, int index, ref CarComponent carComponent, ref Translation translation)
        { 
            if (Mathf.Abs(translation.Value.x - HeroTranslation.Value.x) < carComponent.modelSize.x &&
                Mathf.Abs(translation.Value.z - HeroTranslation.Value.z) < carComponent.modelSize.z)
            {
                // Hit detected
                this.EntityCommandBuffer.DestroyEntity(index, entity);
                this.EntityCommandBuffer.DestroyEntity(index, heroEntity);
            }
        }
    }

    protected override void OnCreate()
    {
        this.entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var hero = this.GetSingletonEntity<HeroComponent>();
        var heroTranslation = this.World.EntityManager.GetComponentData<Translation>(hero);
        var heroSize = this.World.EntityManager.GetComponentData<CarComponent>(hero).modelSize;

        CollisionJob rotationJob = new CollisionJob
        {
            HeroTranslation = heroTranslation,
            heroEntity = hero,
            EntityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        return rotationJob.Schedule(this, inputDeps);
    }
}
