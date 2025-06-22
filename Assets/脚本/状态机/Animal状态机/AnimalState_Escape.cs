using UnityEngine;
[CreateAssetMenu(fileName ="AnimalState_Escape",menuName ="AnimalState/Escape")]
public class AnimalState_Escape : AnimalState
{
    public override void Enter(int id)
    {
        base.Enter(id);
    }
    public override void LogicUpdate(int id)
    {
        base.LogicUpdate(id);
        bool b = false;
        foreach (var a in AnimalMovement.animalsInHorizon)
            if (a != null && a.AnimalType != null && a.AnimalType.attack > AnimalMovement.AnimalType.attack && a.Hungry)
            {
                if (!b)
                {
                    AnimalMovement.destination = Vector2.zero;
                    b = true;
                }
                float x = a.transform.position.x - AnimalMovement.transform.position.x, y = a.transform.position.y - AnimalMovement.transform.position.y;
                AnimalMovement.destination += new Vector2(x, y).normalized;
            }
        if (b) AnimalMovement.destination = new Vector2(AnimalMovement.transform.position.x, AnimalMovement.transform.position.y) - 1.2f * AnimalMovement.AnimalType.range * AnimalMovement.destination.normalized;
        else if (AnimalMovement.animalCollider.OverlapPoint(AnimalMovement.destination))
            StateMachine.SwitchState(typeof(AnimalState_Idle));
    }
    public override void PhysicUpdate(int id)
    {
        base.PhysicUpdate(id);
        AnimalMovement.ApproachPosition(AnimalMovement.destination, true);
    }

    public override void Exit(int id)
    {
        base.Exit(id);
        AnimalMovement.destination = Vector2.zero;
    }
}
