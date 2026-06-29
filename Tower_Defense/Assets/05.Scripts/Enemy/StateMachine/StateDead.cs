using System;
using Unity;
public class StateDead : StateBase
{
    public StateDead(StateType machinetype, StateMachine machine) : base(machinetype, machine)
    {
    }

    public override Commands Current { get; protected set; }

    public override bool IsExecuteOK => true;

    public override void Execute()
    {
        Current = Commands.Prepare;
        _machine.EnemyObj.IsMovable = false;
    }

    public override void FixedUpdate()
    {
    }

    public override void ForceStop()
    {
        Current = Commands.Idle;
        _animator.SetBool("Dead", false);
    }

    public override StateType Update()
    {
        StateType next = _machineType;
        switch (Current) {
            case Commands.Idle:
                break;
            case Commands.Prepare: {
                    _animator.SetBool("Dead", true);
                    MoveNext();
                }
                break;
            case Commands.Casting:
                    MoveNext();
                break;
            case Commands.OnAction:
                break;
            case Commands.Finish:
                break;
            default:
                break;
        }
        return next;
    }
}