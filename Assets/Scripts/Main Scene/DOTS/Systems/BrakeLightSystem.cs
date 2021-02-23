using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

/// <summary>
/// Handles wheel rotation.
/// </summary>
public class BrakeLightSystem : JobComponentSystem
{
    private BeginInitializationEntityCommandBufferSystem entityCommandBufferSystem;

    private EntityQuery streetCarsGroup;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // TODO: this looks ridicously complex for a similar task.
        var hero = this.GetSingletonEntity<HeroComponent>(); 
        var carData =this.EntityManager.GetComponentData<CarComponent>(hero);
        var brake = this.GetSingletonEntity<BrakeLight>();

        this.EntityManager.SetComponentData<BrakeLight>(brake, new BrakeLight { IsOn = carData.IsBraking });

         Entities
            .ForEach((ref BrakeLight brakeData, ref Translation translation) =>
            {
                translation.Value.z = brakeData.IsOn ? 0 : 1;
            }).Run();

        return default;
    }
    
}