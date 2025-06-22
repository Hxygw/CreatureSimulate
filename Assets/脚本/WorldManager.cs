using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public bool procudctFood;
    public GameObject FoodObject;
    public GameObject AnimalObject;
    readonly static List<GameObject> foodPool_On = new();
    readonly static Queue<GameObject> foodPool_Off = new();
    readonly static List<GameObject> animalPool_On = new();
    readonly static Queue<GameObject> animalPool_Off = new();
    public static int FoodNumMax;
    public static int AnimalNumMax;
    public static int FoodCurrent => foodPool_On.Count;
    public static int AnimalCurrent => animalPool_On.Count;
    public static float WorldRange;
    private float foodTime;
    public static bool Full => AnimalCurrent == AnimalNumMax;
    static WorldManager instance;
   
    [Header("世界半径")]
    [SerializeField] private float worldRange;
    [Header("最大食物数量")]
    [SerializeField] private int foodNum;
    [Header("最大生物数量")]
    [SerializeField] private int animalNum;
    [Header("食物产生间隔(s)")]
    [SerializeField] private float foodApprenceGap;
    [Header("食物产生概率(0--1)")]
    [Range(0,1)] [SerializeField] private float foodApprenceProbability;
    [Header("初始食物数量")]
    [SerializeField] private int foodInitial;
    [Header("初始生物数量")]
    [SerializeField] private int animalInitial;
    static int worldManagerNum = 0;
    private void Awake()
    {
        AnimalType.Setup();



        if (instance == null)
            instance = this;
        FoodNumMax = foodNum;
        AnimalNumMax = animalNum;
        worldManagerNum++;
        WorldRange = worldRange;
        for (int i = 0; i < FoodNumMax; i++)
        {
            foodPool_Off.Enqueue(Instantiate(FoodObject, transform));
        }
        for (int i = 0; i < AnimalNumMax; i++)
        {
            AnimalMovement a = Instantiate(AnimalObject, transform).GetComponent<AnimalMovement>();
            a.id = i;
            animalPool_Off.Enqueue(a.gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        if (worldManagerNum > 1)
            throw new System.Exception("WorldManager重复");
        if (animalInitial > AnimalNumMax || foodInitial > FoodNumMax)
            throw new System.Exception("初始值多于最大值");

        for (int i = 0; i < foodInitial; i++)
            FoodAppear();
        for (int i = 0; i < animalInitial; i++)
            AnimalAppear(AnimalType.AnimalTypes[Random.Range(0, AnimalType.AnimalTypes.Count)]);

        foodTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (procudctFood && !Full && FoodCurrent < FoodNumMax && Time.time - foodTime >= foodApprenceGap)
        {
            foodTime = Time.time;
            if (Random.value <= foodApprenceProbability)
                FoodAppear();
        }

        if (AnimalCurrent + animalPool_Off.Count < AnimalNumMax)
            foodPool_Off.Enqueue(Instantiate(AnimalObject, transform));
    }


    public static void FoodAppear()
    {
        float a = Random.value * Mathf.PI * 2;
        while (foodPool_Off.Peek() == null) foodPool_Off.Dequeue();
        foodPool_Off.Peek().transform.position = new Vector3(Mathf.Cos(a) * WorldRange * Random.value, Mathf.Sin(a) * WorldRange * Random.value);
        foodPool_On.Add(foodPool_Off.Peek());
        foodPool_Off.Dequeue().SetActive(true);
    }

    public static void FoodDisappear(GameObject food)
    {
        foodPool_Off.Enqueue(food);
        food.SetActive(false);
        food.transform.position = Vector3.zero;
        foodPool_On.Remove(food);
    }
    public static void AnimalAppear(AnimalType animalType, float x, float y, float satiety = 0.6f)
    {
        if (animalType == null) return;
        while (animalPool_Off.Count != 0 && animalPool_Off.Peek() == null) animalPool_Off.Dequeue();
        if (animalPool_Off.Count == 0) animalPool_Off.Enqueue(Instantiate(instance.AnimalObject, instance.transform));
        animalPool_Off.Peek().transform.position = new Vector3(x, y);
        animalPool_Off.Peek().GetComponent<AnimalMovement>().AnimalType = animalType;
        animalPool_Off.Peek().GetComponent<AnimalMovement>().satiety = satiety;
        animalPool_On.Add(animalPool_Off.Peek());
        animalPool_Off.Dequeue().SetActive(true);
    }
    public static void AnimalAppear(AnimalType animalType, Vector2 vector2, float satiety = 0.6f)
    {
        AnimalAppear(animalType, vector2.x, vector2.y, satiety);
    }
    public static void AnimalAppear(AnimalType animalType)
    {
        if (animalType == null) return;
        float a = Random.value * Mathf.PI * 2;
        AnimalAppear(animalType, Mathf.Cos(a) * WorldRange * Random.value, Mathf.Sin(a) * WorldRange * Random.value);
    }

    public static void AnimalDisappear(GameObject animal)
    {
        animalPool_Off.Enqueue(animal);
        animal.SetActive(false);
        animal.transform.position = Vector3.zero;
        animalPool_On.Remove(animal);
    }

    public static void AnimalDestoryed()
    {
        animalPool_Off.Enqueue(Instantiate(instance.AnimalObject, instance.transform));
    }
    public static void FoodDestoryed()
    {
        foodPool_Off.Enqueue(Instantiate(instance.FoodObject, instance.transform));
    }
}
