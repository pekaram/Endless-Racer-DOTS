using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Collections.Generic;
using System;

public class SystemManager : MonoBehaviour 
{
    /// <summary>
    /// The player's car
    /// </summary>
    [SerializeField]
    private GameObject heroPrefab;

    /// <summary>
    /// Street cars that act as obstacles to avoid, currently they are only 1 type.
    /// </summary>
    [SerializeField]
    private GameObject streetCarPrefab;

    /// <summary>
    /// A generation place on asphalt 
    /// </summary>
    [SerializeField]
    private GameObject startSlotPrefab;

    /// <summary>
    /// The car speed UI
    /// </summary>
    [SerializeField]
    private Text speedText;

    /// <summary>
    /// The street gameobjecgt
    /// </summary>
    [SerializeField]
    private GameObject street;
 
    /// <summary>
    /// Reference to world's entity manager.
    /// </summary>
    private EntityManager entityManager;

    /// <summary>
    /// Hero entity reference, used to easily pick it.
    /// </summary>
    private Entity hero;

    /// <summary>
    /// Hero id used to indentify hero's car
    /// </summary>
    private Guid heroId = Guid.NewGuid();
    
    /// <summary>
    /// Hero's car's box collider size.
    /// </summary>
    private Vector3 heroBoxColliderSize;

    /// <summary>
    /// Street car's box collider size.
    /// </summary>
    private Vector3 streetCarBoxColliderSize;

    /// <summary>
    /// The size of <see cref="streetCarPrefab"/>'s collider.
    /// </summary>
    private CapsuleColliderData streetCarCapsuleData;

    /// <summary>
    /// All street cars excluding hero's car
    /// </summary>
    private List<Entity> streetCars = new List<Entity>();

    public static int numberOfGenerationSlots = 5;

    private void Awake()
    {
        this.heroBoxColliderSize = this.GetBoxColliderSize(this.heroPrefab);
        this.streetCarBoxColliderSize = this.GetBoxColliderSize(this.streetCarPrefab);
        this.streetCarCapsuleData = this.GetCapusleSize(this.streetCarPrefab);
    }

    private void Start()
    {
        entityManager = World.Active.EntityManager;
        this.AddHero();
        this.CreateStreetCars();
        this.CreateStartingSlots();
    }

    private Vector3 GetBoxColliderSize(GameObject targetPrefab)
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

    private CapsuleColliderData GetCapusleSize(GameObject targetPrefab)
    {
        var modelObject = Instantiate(targetPrefab);
        var collider = modelObject.GetComponent<CapsuleCollider>();
        if (collider == null)
        {
            Debug.LogError("No capsule collider was found attached to this object");
        }

        Destroy(modelObject);
        return new CapsuleColliderData { Height = collider.height, Radius = collider.radius };
    }

    private void CreateStartingSlots()
    {
        for (var i = 0; i <= Settings.NumberOfGenerationSlots; i++)
        {
            var entity = this.CreateEntityFromPrefab(this.startSlotPrefab);
            var creationposition = this.entityManager.GetComponentData<Translation>(entity).Value;
            var shiftedPosition = new Translation { Value = new float3(creationposition.x - (i * 2), creationposition.y, creationposition.z) };
            this.entityManager.SetComponentData<Translation>(entity, shiftedPosition);
            this.entityManager.AddComponentData(entity, new GenerationSlotComponent { Position = shiftedPosition });
        }
    }

    private void AddHero()
    {
        this.hero = this.CreateEntityFromPrefab(this.heroPrefab);
        this.entityManager.AddComponentData(this.hero, new HeroComponent());
        var carPosition = this.entityManager.GetComponentData<Translation>(this.hero);
        carPosition.Value.z -= 2;
        this.entityManager.SetComponentData<Translation>(this.hero, carPosition);
        this.entityManager.AddComponentData(this.hero, new CarComponent() { ID = this.heroId, CubeColliderSize = this.heroBoxColliderSize});        
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
            var carEntity = this.CreateEntityFromPrefab(this.streetCarPrefab);
            var carPosition = this.entityManager.GetComponentData<Translation>(carEntity);
            this.streetCars.Add(carEntity);
            carPosition.Value.x -= 4f * i;
            carPosition.Value.z += 1;
            this.entityManager.SetComponentData<Translation>(carEntity, carPosition);
            this.entityManager.AddComponentData(carEntity, new CarComponent { ID = Guid.NewGuid(), Speed = 20, CubeColliderSize = this.streetCarBoxColliderSize, CapsuleColliderData = this.streetCarCapsuleData });
        }
    }

    /// <summary>
    /// A generic add component
    /// </summary>
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

    /// <summary>
    /// Polling for game end
    /// </summary>
    private void Update()
    {
        var didEnd = this.entityManager.GetComponentData<CarComponent>(this.hero).IsCollided;   
        if (!didEnd)
        {
            return;
        }

        this.enabled = false;

        foreach(var system in this.entityManager.World.Systems)
        {
            system.Enabled = false;
        }
    }

    /// <summary>
    /// Scrolls the street for moving world
    /// </summary>
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
