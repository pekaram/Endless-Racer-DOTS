using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class SystemManager : MonoBehaviour 
{
    [SerializeField]
    private GameObject heroPrefab;

    [SerializeField]
    private GameObject streetCarPrefab;

    [SerializeField]
    private GameObject startSlotPrefab;

    [SerializeField]
    private Text speedText;

    [SerializeField]
    private GameObject street;

    private EntityManager entityManager;

    private Entity hero;

    private Entity streetCar;

    private void Start()
    {
        entityManager = World.Active.EntityManager;
        this.AddHero();
        this.CreateStreetCars();
        this.CreateStartingSlots();
    }

    private void CreateStartingSlots()
    {
        for (var i = 0; i < 4; i++)
        {
            var entity = this.CreateEntityFromPrefab(this.startSlotPrefab);
            var creationposition = this.entityManager.GetComponentData<Translation>(entity).Value;
            var shiftedPosition = new Translation { Value = new float3(creationposition.x - i * 2, creationposition.y, creationposition.z) };
            this.entityManager.SetComponentData<Translation>(entity, shiftedPosition);
            this.entityManager.AddComponentData(entity, new GenerationSlotComponent { ReadOnlyPosition = shiftedPosition });
        }
    }

    private void AddHero()
    {
        this.hero = this.CreateEntityFromPrefab(this.heroPrefab);
        this.entityManager.AddComponentData(this.hero, new HeroComponent());
        this.entityManager.AddComponentData(this.hero, new CarComponent());
    }

    private Entity CreateEntityFromPrefab(GameObject prefab)
    {
        var convertedPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, World.Active);
        var entity = entityManager.Instantiate(convertedPrefab);
        return entity;
    }

    private void CreateStreetCars()
    {
        for(var i = 0; i < 2; i++)
        {
            var carEntity = this.CreateEntityFromPrefab(streetCarPrefab);
            var carPosition = this.entityManager.GetComponentData<Translation>(carEntity);
            carPosition.Value.x -= 4f * i;
            this.entityManager.SetComponentData<Translation>(carEntity, carPosition);
            this.entityManager.AddComponentData(carEntity, new CarComponent { Speed = 0.1f + 0.2f * i });
        }
    }

    private void AddComponentGeneric(Entity entity, IComponentData component)
    {
        MethodInfo methodInfo = typeof(EntityManager).GetMethod("AddComponentData");
        MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(component.GetType());
        var parameters = new object[] {entity, component};
        object componentData = genericMethodInfo.Invoke(this.entityManager, parameters);
    }

    private void Update()
    {
        var data = this.entityManager.GetComponentData<CarComponent>(hero);
        speedText.text = Mathf.RoundToInt(data.Speed).ToString();
        this.UpdateStreet();
    }

    private void UpdateStreet()
    {
        var data = this.entityManager.GetComponentData<CarComponent>(hero);
        speedText.text = Mathf.RoundToInt(data.Speed).ToString();

        if (street.transform.position.z > -12)
        {
            street.transform.Translate(0, 0, -data.Speed/1000);
        }
        else
        {
            street.transform.position = new Vector3(0, 0, 0);
        }
    }
}
