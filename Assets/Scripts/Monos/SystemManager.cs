using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Collections.Generic;

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

    public Vector3 heroSize;

    public Vector3 streetCarSize;

    private List<Entity> streetCars = new List<Entity>();

    private void Awake()
    {
        this.heroSize = this.GetModelSizeFromCollider(this.heroPrefab);
        this.streetCarSize = this.GetModelSizeFromCollider(this.streetCarPrefab);
    }

    private void Start()
    {
        entityManager = World.Active.EntityManager;
        this.AddHero();
        this.CreateStreetCars();
        this.CreateStartingSlots();
    }

    private Vector3 GetModelSizeFromCollider(GameObject targetPrefab)
    {
        var modelObject = Instantiate(targetPrefab);
        var collider = modelObject.GetComponent<BoxCollider>();
        if (collider == null)
        {
            Debug.LogError("No box collider was found attached to this object");
        }

        var size = collider.bounds.size;
        Destroy(modelObject);
        return size;
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
        var carPosition = this.entityManager.GetComponentData<Translation>(this.hero);
        carPosition.Value.z -= 2;
        this.entityManager.SetComponentData<Translation>(this.hero, carPosition);
        this.entityManager.AddComponentData(this.hero, new CarComponent() { modelSize = this.heroSize});        
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
            this.streetCars.Add(carEntity);
            carPosition.Value.x -= 4f * i;
            carPosition.Value.z += 1;
            this.entityManager.SetComponentData<Translation>(carEntity, carPosition);
            this.entityManager.AddComponentData(carEntity, new CarComponent { Speed = 20, modelSize = this.streetCarSize });
        }
    }

    private void AddComponentGeneric(Entity entity, IComponentData component)
    {
        MethodInfo methodInfo = typeof(EntityManager).GetMethod("AddComponentData");
        MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(component.GetType());
        var parameters = new object[] {entity, component};
        object componentData = genericMethodInfo.Invoke(this.entityManager, parameters);
    }

    private void FixedUpdate()
    {
        var data = this.entityManager.GetComponentData<CarComponent>(hero);
        speedText.text = Mathf.RoundToInt(data.Speed).ToString();
        this.UpdateStreet();
    }

    private void Update()
    {
        // Improve this to be more straight forward.
        var didEnd = false;
        foreach(var car in this.streetCars)
        {
            if (this.entityManager.GetComponentData<CarComponent>(car).IsDestroyed)
            {
                didEnd = true;
                break;
            }
        }

        if(!didEnd)
        {
            return;
        }

        this.enabled = false;

        foreach(var system in this.entityManager.World.Systems)
        {
            system.Enabled = false;
        }
    }

    private void UpdateStreet()
    {
        var data = this.entityManager.GetComponentData<CarComponent>(hero);
        speedText.text = Mathf.RoundToInt(data.Speed).ToString();

        if (street.transform.position.z > -12)
        {
            street.transform.Translate(0, 0, -data.Speed/100);
        }
        else
        {
            street.transform.position = new Vector3(0, 0, 0);
        }
    }
}
