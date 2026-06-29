public interface IState
{
    bool IsExecuteOK { get; }
    Commands Current { get; }

    void Execute();
    void ForceStop();
    StateType Update();
    void FixedUpdate();
    void MoveNext();
}
