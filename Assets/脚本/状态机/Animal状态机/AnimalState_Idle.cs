using UnityEngine;
[CreateAssetMenu(fileName ="AnimalState_Idle",menuName ="AnimalState/Idle")]
public class AnimalState_Idle : AnimalState
{
    public override void Enter(int id)
    {
        base.Enter(id);
        SetDestination();
        AnimalMovement.target = null;
    }
    public override void LogicUpdate(int id)
    {
        base.LogicUpdate(id);
        if (AnimalMovement.Hunting && !AnimalMovement.Tired)
            foreach (var a in AnimalMovement.animalsInTouch)
                if (a != null && a.AnimalType != null && a.Hungry && a.AnimalType.attack == AnimalMovement.AnimalType.attack && Random.value < AnimalMovement.satiety / (AnimalMovement.satiety + a.satiety))
                {
                    AnimalMovement.satiety += a.satiety;
                    a.Dead();
                }
        foreach (var animal in AnimalMovement.animalsInHorizon)
            if (animal.AnimalType != null && animal.Hunting && animal.AnimalType.attack > AnimalMovement.AnimalType.attack)
            {
                StateMachine.SwitchState(typeof(AnimalState_Escape));
                return;
            }
            else if ((AnimalMovement.AnimalType.foodHabit == FoodHabit.Carnivorous || AnimalMovement.AnimalType.foodHabit == FoodHabit.Omnivorous) && !AnimalMovement.Tired && animal.AnimalType != null && animal.AnimalType.attack < AnimalMovement.AnimalType.attack)
                if((animal.transform.position - AnimalMovement.transform.position).sqrMagnitude < (AnimalMovement.target?.position ?? new Vector3(10000, 10000, 10000) - AnimalMovement.transform.position).sqrMagnitude)
                    AnimalMovement.target = animal.transform;
        if (AnimalMovement.target != null)
        {
            StateMachine.SwitchState(typeof(AnimalState_FindFood));
            return;
        }
        if (AnimalMovement.ReadyForLove && !WorldManager.Full)
        {
            foreach (var a in AnimalMovement.animalsInHorizon)
                if (a.ReadyForLove && a.AnimalType.foodHabit == AnimalMovement.AnimalType.foodHabit)
                {
                    AnimalMovement.target = a.transform;
                    a.target = AnimalMovement.transform;
                    StateMachine.SwitchState(typeof(AnimalState_FindLove));
                    a.AnimalStateMachine.SwitchState(typeof(AnimalState_FindLove));
                }
        }
        else if ((AnimalMovement.AnimalType.foodHabit == FoodHabit.herbivore || AnimalMovement.AnimalType.foodHabit == FoodHabit.Omnivorous) && AnimalMovement.foodsInHorizon.Count != 0)
        {
            foreach (var a in AnimalMovement.foodsInHorizon)
                if ((a.transform.position - AnimalMovement.transform.position).sqrMagnitude < (AnimalMovement.target?.position ?? new Vector3(10000, 10000, 10000) - AnimalMovement.transform.position).sqrMagnitude)
                    AnimalMovement.target = a.transform;
            StateMachine.SwitchState(typeof(AnimalState_FindFood));
            return;
        }
        if (AnimalMovement.animalCollider.OverlapPoint(AnimalMovement.destination))
            SetDestination();
    }

    public override void PhysicUpdate(int id)
    {
        base.PhysicUpdate(id);
        AnimalMovement.ApproachPosition(AnimalMovement.destination, false);
    }
    public override void Exit(int id)
    {
        base.Exit(id);
        //AnimalMovement.destination = Vector2.zero;
    }

    protected void SetDestination()
    {
        float a = Random.value * Mathf.PI * 2;
        AnimalMovement.destination = new Vector2(AnimalMovement.transform.position.x + Mathf.Cos(a) * AnimalMovement.AnimalType.range, AnimalMovement.transform.position.y + Mathf.Sin(a) * AnimalMovement.AnimalType.range);
        if (AnimalMovement.destination.x > WorldManager.WorldRange) AnimalMovement.destination.x = WorldManager.WorldRange;
        if (AnimalMovement.destination.x < -WorldManager.WorldRange) AnimalMovement.destination.x = -WorldManager.WorldRange;
        if (AnimalMovement.destination.y > WorldManager.WorldRange) AnimalMovement.destination.y = WorldManager.WorldRange;
        if (AnimalMovement.destination.y < -WorldManager.WorldRange) AnimalMovement.destination.y = -WorldManager.WorldRange;
    }
}
