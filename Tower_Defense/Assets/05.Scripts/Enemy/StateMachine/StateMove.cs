using Space;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class StateMove : StateBase
{
    public StateMove(StateType machinetype, StateMachine machine) : base(machinetype, machine)
    {

    }

    public override Commands Current { get; protected set; }

    public override bool IsExecuteOK => true;

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
            case Commands.Prepare: {
                    //여기서 실행할 듯
                    MoveNext();
                }
                break;
            case Commands.Casting: {
                    MoveNext();
                }
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
