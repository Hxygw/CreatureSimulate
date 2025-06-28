using UnityEngine;
using System.Collections.Generic;
public class AnimalMovement : MonoBehaviour
{
    /// <summary>
    /// ����ٶ���������
    /// </summary>
    const float RushSpeedFix = 200;
    /// <summary>
    /// �����ٶ���������
    /// </summary>
    const float RunSpeedFix = 50;
    /// <summary>
    /// �����ٶ���������
    /// </summary>
    const float IdleSpeedFix = 10;
    /// <summary>
    /// ����ٶ���������
    /// </summary>
    const float RushAccelerationFix = 280;
    /// <summary>
    /// ���ܼ��ٶ���������
    /// </summary>
    const float RunAccelerationFix = 200;
    /// <summary>
    /// �������ٶ���������
    /// </summary>
    const float IdleAccelerationFix = 150;
    /// <summary>
    /// ƣ�ͼ����ٶ���������
    /// </summary>
    const int RunHungrySpeedFix = 5000000;
    /// <summary>
    /// �����ٶ���������
    /// </summary>
    const int HungrySpeedFix = 50000000;
    /// <summary>
    /// ��С��������
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
    Vector3 velocity;
    Vector2 AccelerationDirection;
    bool rest = false;
    /// <summary>
    /// ɢ��Ŀ�ĵ�
    /// </summary>
    /*[HideInInspector]*/ public Vector2 destination = Vector2.zero;
    [HideInInspector] public float tiredStartTime;
    [Header("ƣ�ͳ���ʱ��(s)")]
    public float tiredTime;
    [Header("��ʳ��(0--1)")]
    [Range(0, 1)] public float satiety;
    [Header("����״̬�ı�ʳ��")]
    [Range(0,1)]public float satiety_findfood;
    [Header("��ʼѰ�Ұ��µı�ʳ��ˮƽ")]
    [Range(0, 1)] public float satiety_findlove;
    [Header("��ʼ��ʳ��")]
    [Range(0, 1)] public float satiety_initial;
    [Header("������Ұ��ײ��")]
    public CircleCollider2D animalHorizon;
    [Header("���������ײ��")]
    public CircleCollider2D animalCollider;
    [Header("ƣ���ٶ�˥������(0--1)")]
    [Range(0, 1)] public float tiredSpeedCoast;
    public Transform target = null;
    public bool FindingLove => AnimalStateMachine.currentState.GetType() == typeof(AnimalState_FindLove);
    public bool ReadyForLove => satiety >= satiety_findlove && !FindingLove && Time.time - tiredStartTime > tiredTime;
    public bool Hungry => satiety <= satiety_findfood;
    public bool Tired => Time.time - tiredStartTime <= tiredTime;
    public bool Hunting => !Tired && (AnimalType.foodHabit == FoodHabit.Carnivorous || AnimalType.foodHabit == FoodHabit.Omnivorous && Hungry);
    public bool Idle => AnimalStateMachine.currentState.GetType() == typeof(AnimalState_Idle);

    /// <summary>
    /// ��Ұ��Ķ���
    /// </summary>
    [HideInInspector] public List<AnimalMovement> animalsInHorizon = new();
    /// <summary>
    /// �����Ķ���
    /// </summary>
    [HideInInspector] public List<AnimalMovement> animalsInTouch = new();
    /// <summary>
    /// ��Ұ���ʳ��
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
        AnimalRigidBody.mass = AnimalType.weight;
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
                else if ((AnimalType.foodHabit == FoodHabit.Omnivorous || AnimalType.foodHabit == FoodHabit.herbivore) && colliders[i].TryGetComponent(out Food f))
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
        if (animalCollider.OverlapPoint(destination))
            destination = Vector2.zero;
        if (rest) breath += Time.fixedDeltaTime * WorldManager.TimeScale;
        if (breath <= 0) rest = true;
        if (breath > AnimalType.breath)
        {
            breath = AnimalType.breath;
            rest = false;
        }
        satiety -= AnimalType.hungrySpeed / (breath < AnimalType.breath ? RunHungrySpeedFix : HungrySpeedFix) * WorldManager.TimeScale;
        if (satiety < 0)
            Dead();
    }

    public void Dead()
    {
        AnimalStateMachine.SwitchState(typeof(AnimalState_Dead));
        AnimalType = null;
        WorldManager.AnimalDisappear(gameObject);
    }

    public void ApproachPosition(Vector2 position, bool rush)
    {
        if (WorldManager.TimeScale == 0)
        {
            AnimalRigidBody.velocity = Vector3.zero;
            return;
        }
        if (!rush && breath < AnimalType.breath) rest = true;
        if (rush && !rest)
            breath -= Time.fixedDeltaTime * WorldManager.TimeScale;
        if (rest)
            rush = false;

        float x = position.x - transform.position.x;
        float y = position.y - transform.position.y;
        float f = (Idle ? IdleAccelerationFix : (rush ? RushAccelerationFix : RunAccelerationFix)) * AnimalType.acceleration;
        float v = (Idle ? IdleSpeedFix : (rush ? RushSpeedFix : RunSpeedFix)) * AnimalType.speed;
        if (AccelerationDirection.magnitude == 0 || position != destination || new Vector2(x, y).magnitude % 1 < 0.01f)
            AccelerationDirection = MinimumTimeAcceleration.AccelerationDirection(AnimalRigidBody.velocity, WorldManager.TimeScale * f / AnimalType.weight, x, y);
        if (position != destination)
            destination = position;
        if (AnimalRigidBody.velocity.magnitude == 0)
            AnimalRigidBody.velocity = velocity * WorldManager.TimeScale;
        AnimalRigidBody.AddForce(WorldManager.TimeScale * f * AccelerationDirection);
        if (AnimalRigidBody.velocity.magnitude > v * WorldManager.TimeScale)
            AnimalRigidBody.velocity = WorldManager.TimeScale * v * AnimalRigidBody.velocity.normalized;
        velocity = AnimalRigidBody.velocity / WorldManager.TimeScale;
    }


    private void OnDestroy()
    {
        if (isActiveAndEnabled) Debug.Log("!!!");
        WorldManager.AnimalDestoryed();
    }

}