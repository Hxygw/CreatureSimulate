using System.Collections.Generic;
using UnityEngine;

public class AnimalState : ScriptableObject, Istate
{
    [SerializeField] protected string statename;
    //[SerializeField, Range(0f, 1f)] protected float transitionDuration = 0.1f;
    //protected Animator animator;
    protected List<AnimalStateMachine> stateMachines = new();
    protected AnimalStateMachine StateMachine => stateMachines[currentStateMachineId];

    protected int StateHash;
    protected float StateStartTime;
    protected float StateDuration => Time.time - StateStartTime;
    public int currentStateMachineId;
    //protected bool IsAnimationFinished => StateDuration >= animator.GetCurrentAnimatorStateInfo(0).length;
    protected List<AnimalMovement> animalMovements = new();
    protected AnimalMovement AnimalMovement => animalMovements[currentStateMachineId];

    void OnEnable()
    {
        if (statename.Length >= 1) StateHash = Animator.StringToHash(statename);
        else StateHash = 0;
        stateMachines.Clear();
        animalMovements.Clear();
    }
    /*public void Initialize(Animator animator, AnimalStateMachine stateMachine)
    {
        this.animator = animator;
        this.stateMachine = stateMachine;
    }*/
    public int Initialize(AnimalMovement animalMovement,AnimalStateMachine stateMachine)
    {
        stateMachines.Add(stateMachine);
        animalMovements.Add(animalMovement);
        return stateMachines.Count - 1;
    }
    /// <summary>
    /// 把SwitchAnimation()协程放到base.Enter()前面
    /// </summary>
    public virtual void Enter(int id)
    {
        currentStateMachineId = id;
        StateStartTime = Time.time;
        //if (StateHash != 0) animator.CrossFade(StateHash, transitionDuration);
    }
    public virtual void Exit(int id)
    {
        currentStateMachineId = id;
    }

    public virtual void LogicUpdate(int id)
    {
        currentStateMachineId = id;
    }

    public virtual void PhysicUpdate(int id)
    {
        currentStateMachineId = id;
    }

    //=====================================================================================================================
    //函数


    //=====================================================================================================================
    //协程

}
