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
        if (CheckEscape())
            return;
        if (AnimalMovement.target == null || !AnimalMovement.target.gameObject.activeSelf || (AnimalMovement.target.gameObject.CompareTag("Animal") && !AnimalMovement.animalsInHorizon.Contains(AnimalMovement.target.GetComponent<AnimalMovement>())))
            StateMachine.SwitchState(typeof(AnimalState_Idle));
        if (AnimalMovement.Hunting)
            foreach (var a in AnimalMovement.animalsInTouch)
                if (CanInteract(a) && a.AnimalType.attack < AnimalMovement.AnimalType.attack)
                {
                    AnimalMovement.tiredStartTime = Time.time;
                    AnimalMovement.satiety += a.satiety;
                    a.Dead();
                    StateMachine.SwitchState(typeof(AnimalState_Idle));
                    return;
                }
    }
    public override void PhysicUpdate(int id)
    {
        base.PhysicUpdate(id);
        if (AnimalMovement.target != null) AnimalMovement.ApproachPosition(AnimalMovement.target.transform.position, AnimalMovement.target.CompareTag("Animal"));
    }

    public override void Exit(int id)
    {
        base.Exit(id);
        AnimalMovement.target = null;
    }
}
