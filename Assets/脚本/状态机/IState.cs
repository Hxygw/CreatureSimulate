/// <summary>
/// ���״̬�Ľӿڣ����ĸ����������룬�߼����£�������£��˳�
/// </summary>
public interface Istate
{
    void Enter(int id);
    void Exit(int id);
    void LogicUpdate(int id);
    void PhysicUpdate(int id);

}
