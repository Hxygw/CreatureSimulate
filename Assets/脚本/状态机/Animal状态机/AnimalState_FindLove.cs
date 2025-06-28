using UnityEngine;
[CreateAssetMenu(fileName ="AnimalState_FindLove",menuName ="AnimalState/FindLove")]
public class AnimalState_FindLove : AnimalState
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
        if (AnimalMovement.target == null || !AnimalMovement.target.gameObject.activeSelf || WorldManager.Full || AnimalMovement.Hungry || !AnimalMovement.target.GetComponent<AnimalMovement>().FindingLove)
        {
            StateMachine.SwitchState(typeof(AnimalState_Idle));
            return;
        }
        if (AnimalMovement.animalsInTouch.Contains(AnimalMovement.target.GetComponent<AnimalMovement>()))
            Reproduce(AnimalMovement, AnimalMovement.target.GetComponent<AnimalMovement>());
    }
    public override void PhysicUpdate(int id)
    {
        base.PhysicUpdate(id);
        if (AnimalMovement.target != null) AnimalMovement.ApproachPosition(AnimalMovement.target.transform.position, false);
    }
    public override void Exit(int id)
    {
        base.Exit(id);
        AnimalMovement.target = null;
    }


    static void Reproduce(AnimalMovement animal1,AnimalMovement animal2)
    {
        float satiety = (animal1.AnimalType.strepsiptera ? animal1.satiety : animal1.AnimalType.loveSatietyCoast) + (animal2.AnimalType.strepsiptera ? animal2.satiety : animal2.AnimalType.loveSatietyCoast);
        animal1.satiety -= animal1.AnimalType.loveSatietyCoast;
        animal2.satiety -= animal2.AnimalType.loveSatietyCoast;
        animal1.tiredStartTime = Time.time;
        animal2.tiredStartTime = Time.time;
        animal1.breath = -0.1f;
        animal2.breath = -0.1f;
        if(animal1.AnimalType.strepsiptera||animal2.AnimalType.strepsiptera)
        {
            while (satiety > 0)
            {
                var a = AnimalType.CreatNewAnimal(animal1.AnimalType, animal2.AnimalType);
                if (a != null)
                {
                    WorldManager.AnimalAppear(a, (animal1.transform.position + animal2.transform.position) / 2f, AnimalType.StrepsipteraLoveCoast);
                    satiety -= AnimalType.StrepsipteraLoveCoast;
                }
            }
            if (animal1.AnimalType.strepsiptera) animal1.Dead();
            else animal1.AnimalStateMachine.SwitchState(typeof(AnimalState_Idle));
            if (animal2.AnimalType.strepsiptera) animal2.Dead();
            else animal2.AnimalStateMachine.SwitchState(typeof(AnimalState_Idle));
            return;
        }
        WorldManager.AnimalAppear(AnimalType.CreatNewAnimal(animal1.AnimalType, animal2.AnimalType), (animal1.transform.position + animal2.transform.position) / 2f);
        animal1.AnimalStateMachine.SwitchState(typeof(AnimalState_Idle));
        animal2.AnimalStateMachine.SwitchState(typeof(AnimalState_Idle));
    }
}
