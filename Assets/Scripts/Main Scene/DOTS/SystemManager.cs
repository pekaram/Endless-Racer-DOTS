using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

/// <summary>
/// Originally for creating DOTS components and start/stop systems when game ends
/// TODO: class grew big and now handles UI related and some visuals, consider refactoring out unrelated things.
/// </summary>
public class SystemManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource carSound;

    [SerializeField]
    private AudioSource crashSound;

    [SerializeField]
    public UpperBillboard upperBillboard;

    [SerializeField]
    private UpperBillboard adBillboard;
    
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private Camera followCamera;

    /// <summary>
    /// Reference to the hero prefab along with its children.
    /// </summary>
    [SerializeField]
    private GameObject heroCarPrefab;

    /// <summary>
    /// Street cars that act as obstacles to avoid, currently they are only 1 type.
    /// </summary>
    [SerializeField]
    private GameObject streetCarPrefab;
    
    [SerializeField]
    private List<GameObject> streetCarsPrefabs;

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

    [SerializeField]
    private ExtendedButton acceleratorPedal;

    [SerializeField]
    private ExtendedButton rightButton;

    [SerializeField]
    private ExtendedButton leftButton;

    [SerializeField]
    private ExtendedButton brakeButton;

    [SerializeField]
    private Animator closeCallTextAnimator;

    [SerializeField]
    private Text fPSText;

    [SerializeField]
    private Text timeText;

    [SerializeField]
    private GameObject lossPanel;
    
    [SerializeField]
    private List<Transform> streetParts;

    /// <summary>
    /// The purpose is to play animation once per close call
    /// </summary>
    private int carInCloseCallLastFrame;
    
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
    private int heroId = 0;
    
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

    private int fPS;

    private int closeCallCount;
    
    [SerializeField]
    private Text closeCallCountText;

    [SerializeField]
    private Text totalDistanceText;

    private int distanceCovered;
    private int distanceCoveredKM;

    private void Awake()
    {
        Application.targetFrameRate = -1;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        this.entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        this.heroBoxColliderSize = this.GetBoxColliderSize(this.heroCarPrefab);
        this.streetCarBoxColliderSize = this.GetBoxColliderSize(this.streetCarPrefab);
        this.streetCarCapsuleData = this.GetCapusleSize(this.heroCarPrefab);
        World.DefaultGameObjectInjectionWorld.QuitUpdate = false;
    }

    private void SetInputBasedOnPlatform()
    {
        IGameInput gameInput;
#if UNITY_ANDROID
        gameInput = new AndroidInput(this.acceleratorPedal, this.leftButton, this.rightButton, this.brakeButton);
#endif

#if UNITY_STANDALONE
        gameInput = new PcInput();
#endif

#if UNITY_EDITOR
        gameInput = new PcInput();
#endif
        foreach (var system in this.entityManager.World.Systems)
        {
            if (system is InputSystem inputSystem)
            {
                inputSystem.GameInput = gameInput;
            }
        }
    }

    private void SlowUpdate()
    {
        this.fPSText.text = this.fPS.ToString();
        this.fPS = 0;

        var data = this.entityManager.GetComponentData<CarComponent>(hero);
        var heroTranslation = this.entityManager.GetComponentData<Translation>(hero);

        var roundedSpeed = Mathf.RoundToInt(data.Speed);
        speedText.text = roundedSpeed.ToString();

        var distanceScaler = 10000;
        this.distanceCovered += ((roundedSpeed * distanceScaler) / 60 / 60);
        this.distanceCoveredKM = this.distanceCovered / 1000;

        this.upperBillboard.SpawnIfReady(this.distanceCoveredKM, heroTranslation.Value.z);
        this.adBillboard.SpawnIfReady(this.distanceCoveredKM, heroTranslation.Value.z);
    }

    private void Start()
    {
        this.SetInputBasedOnPlatform();
        this.CreateStreetCars();
        this.CreateStartingSlots();
        this.CreateHeroCar();
        this.CreateStreetCarCatalogue();
        this.InvokeRepeating(nameof(this.SlowUpdate), 0, 1);
    }

    private void CreateStreetCarCatalogue()
    {
        var bufferList = this.entityManager.CreateEntity(typeof(StreetCarPrefabsBuffer));
        this.entityManager.AddBuffer<LinkedEntityGroup>(bufferList);

        for (var i = 0; i < this.streetCarsPrefabs.Count; i++)
        {
            var entity = this.CreateEntityFromGameObject(this.streetCarsPrefabs[i]);
            this.entityManager.AddComponent<Disabled>(entity);
            
            var buffer = this.entityManager.GetBuffer<LinkedEntityGroup>(bufferList);
            buffer.Add(entity);

        }
    }

    public void SwitchCamera()
    {
        this.followCamera.gameObject.SetActive(this.mainCamera.gameObject.activeInHierarchy);
        this.mainCamera.gameObject.SetActive(!this.mainCamera.gameObject.activeInHierarchy);
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
            var shiftedPosition = new Translation { Value = new float3(creationposition.x - (i * 1.7f), creationposition.y, creationposition.z) };
            this.entityManager.SetComponentData<Translation>(entity, shiftedPosition);
            this.entityManager.AddComponentData(entity, new GenerationSlotComponent { Position = shiftedPosition });
        }
    }

    /// <summary>
    /// Creates the hero car.
    /// </summary>
    private void CreateHeroCar()
    {
        var carReferences = Instantiate(this.heroCarPrefab);
        
        this.heroId = 0;
        this.hero = this.CreateCarStructure(carReferences, this.heroId);
        
        this.AddHeroCompnents();
    }

    /// <summary>
    /// Creates a car entity and maintains its children hirerachy in DOTS while seperating children them into seperate entities  
    /// </summary>
    /// <param name="carHirerachyIndex"> to rip the data from </param>
    /// <returns> the parent car </returns>
    private Entity CreateCarStructure(GameObject car, int id)
    {
        var carEntity =  this.CreateEntityFromGameObject(car);
        
        var parts = this.entityManager.GetBuffer<LinkedEntityGroup>(carEntity);
        for (var i = 0; i < parts.Length; i++)
        {
            if(this.entityManager.HasComponent<WheelComponent>(parts[i].Value))
            {
                this.entityManager.SetComponentData(parts[i].Value, new WheelComponent { Parent = carEntity, ParentID = id });
            }
        }

        return carEntity;
    }
    
    /// <summary>
    /// Assigns components for the hero car <see cref="hero"/>
    /// </summary>
    private void AddHeroCompnents()
    {
        this.entityManager.AddComponentData(this.hero, new HeroComponent());
        var carPosition = this.entityManager.GetComponentData<Translation>(this.hero);
        carPosition.Value.z -= 2;
        carPosition.Value.y -= 0.2f;
        this.entityManager.SetComponentData<Translation>(this.hero, carPosition);
        this.entityManager.AddComponentData(this.hero, new CarComponent() { ID = this.heroId, CubeColliderSize = this.heroBoxColliderSize, CapsuleColliderData = this.streetCarCapsuleData, CarInCloseCall = -1});        
    }

    /// <summary>
    /// Creates entity from prefabs
    /// </summary>
    /// <param name="source"> to create from </param>
    /// <returns> entity</returns>
    private Entity CreateEntityFromPrefab(GameObject source)
    {
        var convertedGameObject = GameObjectConversionUtility.ConvertGameObjectHierarchy(source, new GameObjectConversionSettings(World.DefaultGameObjectInjectionWorld, GameObjectConversionUtility.ConversionFlags.GameViewLiveLink));
        return entityManager.Instantiate(convertedGameObject);
    }
    
    /// <summary>
    /// Creates a duplicate ECS entity from a gameobject
    /// </summary>
    /// <param name="source"> to clone </param>
    /// <param name="destroySource"> will destroy the original GameObject and leave the DOTS entity only </param>
    /// <returns> the created entity </returns>
    private Entity CreateEntityFromGameObject(GameObject source, bool destroySource = true)
    {
        if(this.IsGameObjectActive(source))
        {
            Debug.LogError("Use CreateEntityFromPrefab instead");
        }

        var convertedGameObject = GameObjectConversionUtility.ConvertGameObjectHierarchy(source, new GameObjectConversionSettings(World.DefaultGameObjectInjectionWorld, GameObjectConversionUtility.ConversionFlags.GameViewLiveLink));
        Destroy(source);
        return convertedGameObject;
    }

    /// <summary>
    /// A check for making sure objects are active before creating their DOTS entity
    /// </summary>
    /// <returns> true if active or could be activated </returns>
    private bool IsGameObjectActive(GameObject target)
    {
        target.SetActive(true);
        // SetActive won't have any effect on prefabs, unlike scene objects
        // Any gameobject needs to be active before being passed to DOTS.
        return !target.activeInHierarchy;
    }

    private void CreateStreetCars()
    {
        for (var i = 0; i < 7; i++)
        {
            var carEntity = this.CreateCarStructure(this.streetCarPrefab, i);
            var carPosition = this.entityManager.GetComponentData<Translation>(carEntity);
            this.streetCars.Add(carEntity);
            carPosition.Value.x -= 4f;
            carPosition.Value.z += 3 * i;
            
            this.entityManager.SetComponentData<Translation>(carEntity, carPosition);
            this.entityManager.AddComponentData(carEntity, new CarComponent { ID = i + 1, Speed = 0, CubeColliderSize = this.streetCarBoxColliderSize, CapsuleColliderData = this.streetCarCapsuleData, CarInCloseCall = -1 });
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

    private void OnCloseCall()
    {
        this.closeCallCount++;

        // De-activated to avoid GC spikes while doing ToString() over and over
        //this.closeCallTextAnimator.gameObject.SetActive(true);
        //this.closeCallTextAnimator.SetTrigger("Reset");
        //this.closeCallCountText.text = this.closeCallCount.ToString();
    }

    /// <summary>
    /// Polling for game end
    /// </summary>
    private void Update()
    {
        var heroData = this.entityManager.GetComponentData<CarComponent>(hero);
        var heroTranslation = this.entityManager.GetComponentData<Translation>(hero);

        this.UpdateFollowCamera(heroData, heroTranslation);
        this.UpdateTopViewCamera(heroData);

        this.ExtendStreet(heroTranslation);

        this.HandleCloseCalls(heroData);

        this.EndGameIfCollided(heroData);

        this.carSound.transform.position = heroTranslation.Value;
        this.carSound.pitch = 1 + (heroData.Speed / Settings.MaxSpeed) * 3;

        this.fPS++;
    }

    private void UpdateFollowCamera(CarComponent carData, Translation translationToFollow)
    {
        var translatedZ = this.followCamera.transform.position.z + ((carData.Speed / Settings.KMToTranslationUnit) * Time.deltaTime);
        var translatedX = Mathf.Lerp(this.followCamera.transform.position.x, translationToFollow.Value.x, Settings.FollowCameraTurn * Time.deltaTime);
        this.followCamera.transform.position = new Vector3(translatedX, this.followCamera.transform.position.y, translatedZ);
    }

    private void UpdateTopViewCamera(CarComponent carData)
    {
        var translatedZ = this.mainCamera.transform.position.z + ((carData.Speed / Settings.KMToTranslationUnit) * Time.deltaTime);
        this.mainCamera.transform.position = new Vector3(this.mainCamera.transform.position.x, this.mainCamera.transform.position.y, translatedZ);
    }


    private void HandleCloseCalls(CarComponent data)
    {
        var isInCloseCall = data.CarInCloseCall != -1 && this.carInCloseCallLastFrame != data.CarInCloseCall;
        if (isInCloseCall)
        {
            carInCloseCallLastFrame = data.CarInCloseCall;
            this.OnCloseCall();
        }
    }

    private void EndGameIfCollided(CarComponent heroCar)
    {
        var didEnd = heroCar.IsCollided;
        if (!didEnd)
        {
            return;
        }

        this.carSound.Stop();
        this.crashSound.Play();

        this.CancelInvoke();
        this.enabled = false;

        World.DefaultGameObjectInjectionWorld.QuitUpdate = true;

        this.lossPanel.SetActive(true);

        this.closeCallCountText.transform.parent.parent.gameObject.SetActive(true);
        this.closeCallCountText.text = this.closeCallCount.ToString();

        this.UpdateDistanceText();
    }

    private void UpdateDistanceText()
    {
        int value = this.distanceCoveredKM;
        int decimalLength = value.ToString("D").Length + (4 - value.ToString().Length);
        this.totalDistanceText.text = value.ToString("D" + decimalLength.ToString());
    }

    public void RestartGame()
    {
        this.lossPanel.SetActive(false);
        this.entityManager.DestroyEntity(this.entityManager.UniversalQuery);
        World.DefaultGameObjectInjectionWorld.QuitUpdate = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Scrolls the street for moving world
    /// </summary>
    private void ExtendStreet(Translation heroPosition)
    {
        if (this.streetParts[this.streetParts.Count-1].transform.position.z - heroPosition.Value.z < 84) 
        {
            var firstPart = this.streetParts[0];
            this.streetParts.RemoveAt(0);
            this.streetParts.Add(firstPart);
            firstPart.Translate(0, 0, 144);
        }
    }
}
