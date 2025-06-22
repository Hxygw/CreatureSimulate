/// <summary>
/// 玩家状态的接口，有四个函数，进入，逻辑更新，物理更新，退出
/// </summary>
public interface Istate
{
    void Enter(int id);
    void Exit(int id);
    void LogicUpdate(int id);
    void PhysicUpdate(int id);

}
