using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基础状态机，每帧执行当前状态的逻辑更新，较为稳定的固定更新执行当前状态的物理更新，并提供切换状态的函数，保存此状态机的所有状态为字典形式
/// </summary>
public class StateMachine : MonoBehaviour
{
    [HideInInspector]
    public Istate currentState;
    [HideInInspector]
    public Dictionary<System.Type, Istate> stateTable = new();
    [HideInInspector]
    public Dictionary<System.Type, int> ids = new();
    protected virtual void Update()
    {
        currentState.LogicUpdate(ids[currentState.GetType()]);
    }
    protected virtual void FixedUpdate()
    {
        currentState.PhysicUpdate(ids[currentState.GetType()]);
    }
    protected void SwitchOn(Istate newIstate)
    {
        currentState = newIstate;
        currentState.Enter(ids[currentState.GetType()]);
    }
    public void SwitchState(Istate newIstate)
    {
        currentState?.Exit(ids[currentState.GetType()]);
        SwitchOn(newIstate);
    }
    public virtual void SwitchState(System.Type newStaseType)
    {
        SwitchState(stateTable[newStaseType]);
    }
}
