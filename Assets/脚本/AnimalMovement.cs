using UnityEngine;
using System.Collections.Generic;
public class AnimalMovement : MonoBehaviour
{
    /// <summary>
    /// 奔跑速度修正乘数
    /// </summary>
    const float RunSpeedFix = 450;
    /// <summary>
    /// 正常速度修正乘数
    /// </summary>
    const float IdleSpeedFix = 300;
    /// <summary>
    /// 奔跑加速度修正乘数
    /// </summary>
    const float RunAccelerationFix = 280;
    /// <summary>
    /// 正常加速度修正乘数
    /// </summary>
    const float AccelerationFix = 80;
    /// <summary>
    /// 疲劳饥饿速度修正除数
    /// </summary>
    const int RunHungrySpeedFix = 5000000;
    /// <summary>
    /// 饥饿速度修正除数
    /// </summary>
    const int HungrySpeedFix = 50000000;
    /// <summary>
    /// 大小修正除数
    /// </summary>
    const float SizeFix = 14;
    
    public int id;
    public AnimalType AnimalType;
    public AnimalStateMachine AnimalStateMachine;
    public Rigidbody2D AnimalRigidBody;
    public SpriteRenderer spriteRenderer;
    public TrailRenderer trailRenderer;
    public Transform circle;
    public float breath;
    bool rest = false;
    /// <summary>
    /// 散步目的地
    /// </summary>
    /*[HideInInspector]*/ public Vector2 destination = Vector2.zero;
    [HideInInspector] public float tiredStartTime;
    [Header("疲劳持续时间(s)")]
    public float tiredTime;
    [Header("饱食度(0--1)")]
    [Range(0, 1)] public float satiety;
    [Header("饥饿状态的饱食度")]
    [Range(0,1)]public float satiety_findfood;
    [Header("开始寻找伴侣的饱食度水平")]
    [Range(0, 1)] public float satiety_findlove;
    [Header("初始饱食度")]
    [Range(0, 1)] public float satiety_initial;
    [Header("动物视野碰撞箱")]
    public CircleCollider2D animalHorizon;
    [Header("动物体积碰撞箱")]
    public CircleCollider2D animalCollider;
    [Header("疲劳速度衰减乘数(0--1)")]
    [Range(0, 1)] public float tiredSpeedCoast;
    public Transform target = null;
    public bool FindingLove => AnimalStateMachine.currentState.GetType() == typeof(AnimalState_FindLove);
    public bool ReadyForLove => satiety >= satiety_findlove && !FindingLove && Time.time - tiredStartTime > tiredTime;
    public bool Hungry => satiety <= satiety_findfood;
    public bool Tired => Time.time - tiredStartTime <= tiredTime;

    /// <summary>
    /// 视野里的动物
    /// </summary>
    [HideInInspector] public List<AnimalMovement> animalsInHorizon = new();
    /// <summary>
    /// 碰到的动物
    /// </summary>
    [HideInInspector] public List<AnimalMovement> animalsInTouch = new();
    /// <summary>
    /// 视野里的食物
    /// </summary>
    [HideInInspector] public List<Food> foodsInHorizon = new();

    public int AnimalTypeId;
    [SerializeReference] public List<string> bodyParts = new();

    readonly Collider2D[] colliders = new Collider2D[10];
    ContactFilter2D filter = new();

    public void OnEnable()
    {
        bodyParts.Clear();
        AnimalTypeId = AnimalType.id;

        foreach (var part in AnimalType.bodyParts)
            bodyParts.Add(part.name);
        animalHorizon.radius = AnimalType.range;
        spriteRenderer.color = AnimalType.color;
        trailRenderer.startColor = AnimalType.color;
        trailRenderer.endColor = AnimalType.color;
        breath = AnimalType.breath;
        tiredStartTime = Time.time;
        circle.localScale = new Vector3(AnimalType.size / SizeFix, AnimalType.size / SizeFix, 1);
        AnimalStateMachine.SwitchState(typeof(AnimalState_Idle));
    }

    public void Update()
    {
        animalsInHorizon.Clear();
        animalsInTouch.Clear();
        foodsInHorizon.Clear();
        animalHorizon.OverlapCollider(filter, colliders);
        for (int i=0;i<colliders.Length;i++)
        {
            if (colliders[i] != null && colliders[i].gameObject != gameObject && !colliders[i].isTrigger)
                if (colliders[i].TryGetComponent(out AnimalMovement a))
                    animalsInHorizon.Add(a);
                else if (colliders[i].TryGetComponent(out Food f))
                    foodsInHorizon.Add(f);
            colliders[i] = null;
        }
        animalCollider.OverlapCollider(filter, colliders);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null && colliders[i].gameObject != gameObject && !colliders[i].isTrigger)
                if (colliders[i].TryGetComponent(out AnimalMovement a))
                    animalsInTouch.Add(a);
            colliders[i] = null;
        }
    }

    private void FixedUpdate()
    {
        if (rest) breath += Time.fixedDeltaTime;
        if (breath <= 0) rest = true;
        if (breath > AnimalType.breath)
        {
            breath = AnimalType.breath;
            rest = false;
        }
        satiety -= AnimalType.hungrySpeed / (breath < AnimalType.breath ? RunHungrySpeedFix : HungrySpeedFix);
        if (satiety < 0)
            Dead();
    }

    public void Dead()
    {
        AnimalStateMachine.SwitchState(typeof(AnimalState_Dead));
        AnimalType = null;
        WorldManager.AnimalDisappear(gameObject);
    }

    public void ApproachPosition(Vector2 position, bool run)
    {
        float x = position.x - transform.position.x;
        float y = position.y - transform.position.y;
        if (!run && breath < AnimalType.breath) rest = true;
        if (run && !rest)
            breath -= Time.fixedDeltaTime;
        if (rest)
            run = false;
        AnimalRigidBody.AddForce((run ? AccelerationFix : RunAccelerationFix) * AnimalType.acceleration * MinimumTimeAcceleration.AccelerationDirection(AnimalRigidBody.velocity, AnimalType.acceleration * (run ? AccelerationFix : RunAccelerationFix), x, y));
        if (AnimalRigidBody.velocity.magnitude > AnimalType.speed * (run ? IdleSpeedFix : RunSpeedFix))
            AnimalRigidBody.velocity = AnimalType.speed * (run ? IdleSpeedFix : RunSpeedFix) * AnimalRigidBody.velocity.normalized;
    }


    private void OnDestroy()
    {
        if (isActiveAndEnabled) Debug.Log("!!!");
        WorldManager.AnimalDestoryed();
    }
}