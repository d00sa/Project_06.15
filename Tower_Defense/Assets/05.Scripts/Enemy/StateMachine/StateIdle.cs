using UnityEngine;
using Space;
using System.Diagnostics;

public class StateIdle : StateBase
{
    public StateIdle(StateType machinetype, StateMachine machine) : base(machinetype, machine)
    {
    }   

    public override bool IsExecuteOK => true;

    public override Commands Current { get; protected set; }

    public override void Execute()
    {
        Current = Commands.Prepare;
        _machine.IsMoveable = true;
    }

    public override void FixedUpdate()
    {

    }

    public override void ForceStop()
    {
        Current = Commands.Idle;
    }

    public override StateType Update()
    {
        StateType next = _machineType;
        switch (Current) {
            case Commands.Idle:
                break;
            case Commands.Prepare:
                MoveNext();
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
