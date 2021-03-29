using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System;


public class DistanceCullingSystem : JobComponentSystem
{
    private Entity heroEntity;

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        this.heroEntity = this.GetSingletonEntity<HeroComponent>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var heroTranslation = World.EntityManager.GetComponentData<Translation>(this.heroEntity);

        var heroZ = heroTranslation.Value.z;
        return Entities.ForEach((ref CarComponent carComponent, ref Translation translation) =>
        {
            if (translation.Value.z - heroZ > 60)
            {
                translation.Value.y = -999;
            }
            else
            {
                translation.Value.y = 0;
            }
        }).Schedule(inputDeps);

    }
}
