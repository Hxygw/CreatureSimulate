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
        Vector2 destination = Vector2.zero;
        foreach (var a in AnimalMovement.animalsInHorizon)
            if (CanInteract(a) && a.Hunting && a.AnimalType.attack > AnimalMovement.AnimalType.attack)
            {
                b = true;
                float x = a.transform.position.x - AnimalMovement.transform.position.x;
                float y = a.transform.position.y - AnimalMovement.transform.position.y;
                destination += new Vector2(x, y).normalized;
            }
        if (b) AnimalMovement.destination = new Vector2(AnimalMovement.transform.position.x, AnimalMovement.transform.position.y) - 2f * AnimalMovement.AnimalType.range * destination.normalized;
        else if (AnimalMovement.destination == Vector2.zero)
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
    }
}
