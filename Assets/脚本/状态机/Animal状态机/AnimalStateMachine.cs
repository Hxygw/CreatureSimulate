using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimalStateMachine : StateMachine
{
    [SerializeField] AnimalState[] states;
    AnimalMovement animalMovement;
    public Dictionary<Type, AnimalState> stateDic;
    //Animator animator;
    private void Awake()
    {
        //animator = GetComponentInChildren<Animator>();
        animalMovement = GetComponent<AnimalMovement>();

        stateTable = new Dictionary<Type, Istate>(states.Length);
        stateDic = new Dictionary<Type, AnimalState>(states.Length);
        ids = new Dictionary<Type, int>(states.Length);
        //Íæ¼Ò³õÊ¼»¯
        foreach (AnimalState state in states)
        {
            //state.Initialize(animator, this);
            ids.Add(state.GetType(), state.Initialize(animalMovement, this));
            stateTable.Add(state.GetType(), state);
            stateDic.Add(state.GetType(), state);
        }
        SwitchOn(stateTable[typeof(AnimalState_Dead)]);
    }
    private void Start()
    {
        //SwitchOn(stateTable[typeof(AnimalState_Dead)]);
    }

    protected override void Update()
    {
        foreach (AnimalState state in states)
            state.currentStateMachineId = ids[state.GetType()];
        base.Update();
    }
    protected override void FixedUpdate()
    {
        foreach (AnimalState state in states)
            state.currentStateMachineId = ids[state.GetType()];
        base.FixedUpdate();
    }
    public override void SwitchState(Type newStaseType)
    {
        var c = currentState?.GetType() ?? typeof(AnimalState_Idle);
        int? idc = stateDic[c].currentStateMachineId, idn = stateDic[newStaseType].currentStateMachineId;
        base.SwitchState(newStaseType);
        stateDic[newStaseType].currentStateMachineId = idn ?? -1;
        stateDic[c ?? typeof(AnimalState_Idle)].currentStateMachineId = idc ?? -1;
    }
}
