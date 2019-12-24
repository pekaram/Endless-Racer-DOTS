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
    [ExcludeComponent(typeof(HeroComponent))]
    [BurstCompile]
    struct CollisionJob : IJobForEach<CarComponent, Translation>
    {
        public Translation HeroTranslation;

        public void Execute(ref CarComponent carComponent, ref Translation translation)
        {
            if (Mathf.Abs(translation.Value.x - HeroTranslation.Value.x) < 1.1 && Mathf.Abs(translation.Value.z - HeroTranslation.Value.z) < 2.6)
            {
               // Debug.Log("hit");
            }

            //Debug.Log(HeroTranslation.Value.x);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var hero = this.GetSingletonEntity<HeroComponent>();
        var heroTranslation = this.World.EntityManager.GetComponentData<Translation>(hero);
        CollisionJob rotationJob = new CollisionJob
        {
            HeroTranslation = heroTranslation
        };
        return rotationJob.Schedule(this, inputDeps);
    }
}
