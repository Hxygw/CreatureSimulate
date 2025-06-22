using UnityEngine;
[CreateAssetMenu(fileName ="AnimalState_FindFood",menuName ="AnimalState/FindFood")]
public class AnimalState_FindFood : AnimalState
{
    public override void Enter(int id)
    {
        base.Enter(id);
    }
    public override void LogicUpdate(int id)
    {
        base.LogicUpdate(id);
        foreach (var animal in AnimalMovement.animalsInHorizon)
            if (animal.AnimalType != null && animal.Hungry && animal.AnimalType.attack > AnimalMovement.AnimalType.attack)
            {
                StateMachine.SwitchState(typeof(AnimalState_Escape));
                return;
            }
        if (AnimalMovement.target == null || !AnimalMovement.target.gameObject.activeSelf || (AnimalMovement.target.gameObject.CompareTag("Animal") && !AnimalMovement.animalsInHorizon.Contains(AnimalMovement.target.GetComponent<AnimalMovement>())))
            StateMachine.SwitchState(typeof(AnimalState_Idle));
        if (AnimalMovement.Hungry && AnimalMovement.animalsInTouch.Count != 0)
            foreach (var a in AnimalMovement.animalsInTouch)
                if (a.AnimalType != null && a.AnimalType.attack < AnimalMovement.AnimalType.attack)
                {
                    StateMachine.SwitchState(typeof(AnimalState_Idle));
                    AnimalMovement.tiredStartTime = Time.time;
                    AnimalMovement.satiety += a.satiety;
                    a.Dead();
                    break;
                }
    }
    public override void PhysicUpdate(int id)
    {
        base.PhysicUpdate(id);
        if (AnimalMovement.target != null) AnimalMovement.ApproachPosition(AnimalMovement.target.transform.position, AnimalMovement.Hungry);
    }

    public override void Exit(int id)
    {
        base.Exit(id);
        AnimalMovement.target = null;
    }
}
