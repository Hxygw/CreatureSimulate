using UnityEngine;
using System.Collections.Generic;
public class AnimalMovement : MonoBehaviour
{
    /// <summary>
    /// �����ٶ���������
    /// </summary>
    const float RunSpeedFix = 450;
    /// <summary>
    /// �����ٶ���������
    /// </summary>
    const float IdleSpeedFix = 300;
    /// <summary>
    /// ���ܼ��ٶ���������
    /// </summary>
    const float RunAccelerationFix = 280;
    /// <summary>
    /// �������ٶ���������
    /// </summary>
    const float AccelerationFix = 80;
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