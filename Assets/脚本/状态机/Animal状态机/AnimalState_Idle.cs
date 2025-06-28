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
        if (CheckEscape())
            return;
        if (CheckHunt())
            return;
        if (AnimalMovement.ReadyForLove && !WorldManager.Full && CheckFindlove())
            return;
        if (CheckFindfood())
            return;
        if (AnimalMovement.destination == Vector2.zero)
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
        Vector2 destination = new(AnimalMovement.transform.position.x + Mathf.Cos(a) * AnimalMovement.AnimalType.range, AnimalMovement.transform.position.y + Mathf.Sin(a) * AnimalMovement.AnimalType.range);
        if (destination.x > WorldManager.WorldRange) destination.x = WorldManager.WorldRange;
        if (destination.x < -WorldManager.WorldRange) destination.x = -WorldManager.WorldRange;
        if (destination.y > WorldManager.WorldRange) destination.y = WorldManager.WorldRange;
        if (destination.y < -WorldManager.WorldRange) destination.y = -WorldManager.WorldRange;
        AnimalMovement.destination = destination;
    }

    protected bool CheckHunt()
    {
        if(!AnimalMovement.Hunting)
            return false;
        foreach (var animal in AnimalMovement.animalsInHorizon)
            if (CanInteract(animal) && animal.AnimalType.attack < AnimalMovement.AnimalType.attack)
                if ((animal.transform.position - AnimalMovement.transform.position).magnitude < (AnimalMovement.target?.position ?? new Vector3(10000, 10000, 10000) - AnimalMovement.transform.position).magnitude)
                    AnimalMovement.target = animal.transform;
        if (AnimalMovement.target != null)
        {
            StateMachine.SwitchState(typeof(AnimalState_FindFood));
            return true;
        }
        return false;
    }

    protected bool CheckFindlove()
    {
        if (!AnimalMovement.ReadyForLove)
            return false;
        foreach (var a in AnimalMovement.animalsInHorizon)
            if (CanInteract(a) && a.ReadyForLove && a.AnimalType.foodHabit == AnimalMovement.AnimalType.foodHabit)
            {
                AnimalMovement.target = a.transform;
                a.target = AnimalMovement.transform;
                StateMachine.SwitchState(typeof(AnimalState_FindLove));
                a.AnimalStateMachine.SwitchState(typeof(AnimalState_FindLove));
                return true;
            }
        return false;
    }

    protected bool CheckFindfood()
    {
        if (AnimalMovement.AnimalType.foodHabit == FoodHabit.Carnivorous || AnimalMovement.foodsInHorizon.Count == 0)
            return false;
        foreach (var f in AnimalMovement.foodsInHorizon)
            if (CanInteract(f) && (f.transform.position - AnimalMovement.transform.position).sqrMagnitude < (AnimalMovement.target?.position ?? new Vector3(10000, 10000, 10000) - AnimalMovement.transform.position).sqrMagnitude)
                AnimalMovement.target = f.transform;
        StateMachine.SwitchState(typeof(AnimalState_FindFood));
        return false;
    }
}
