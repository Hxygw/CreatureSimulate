using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����״̬����ÿִ֡�е�ǰ״̬���߼����£���Ϊ�ȶ��Ĺ̶�����ִ�е�ǰ״̬��������£����ṩ�л�״̬�ĺ����������״̬��������״̬Ϊ�ֵ���ʽ
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
