using UnityEngine;
using System.Diagnostics;

public class StateIdle : StateBase
{
    public StateIdle(StateType machinetype, StateMachine machine) : base(machinetype, machine)
    {
    }   

    public override bool IsExecuteOK => !_machine.EnemyObj.IsMovable;

    public override Commands Current { get; protected set; }

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
        _animator.SetBool("Idle", false);
    }

    public override StateType Update()
    {
        StateType next = _machineType;
        switch (Current) {
            case Commands.Idle:
                break;
            case Commands.Prepare: 
                {
                    _animator.SetBool("Idle", true);
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
        }
        return next;
    }
}
