using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

public class CarMovementSystem : JobComponentSystem
{
    BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;
    
    [BurstCompile]
    struct MovementJob : IJobForEach<CarComponent, Translation>
    {
        public float HeroSpeed;

        private const float Speed = 0.1f;

        public const float KMToTranslationUnit = 1000;

        public EntityCommandBuffer.Concurrent EntityCommandBuffer;
      
        public void Execute(ref CarComponent carComponent,ref Translation translation)
        {
            if (translation.Value.z > -9.5f)
            {
                translation.Value.z -= (HeroSpeed - carComponent.Speed) /  MovementJob.KMToTranslationUnit;
            }
            else
            {
                carComponent.IsDisabled = true;
            }
        }
    }

    protected override void OnCreate()
    {
        entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var heroEntity = this.GetSingletonEntity<HeroComponent>();

        MovementJob movementJob = new MovementJob
        {
            HeroSpeed = this.World.EntityManager.GetComponentData<HeroComponent>(heroEntity).Speed,
            EntityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
        };
        
        return movementJob.Schedule(this, inputDeps);
    }
}
