using Space;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public abstract class StateBase : IState
{
    protected StateMachine _machine;
    protected StateType _machineType;
    protected Animator _animator;
    public abstract Commands Current { get; protected set; }
    public StateBase(StateType machinetype, StateMachine machine)
    {
        _machine = machine;
        _machineType = machinetype;
        _animator = _machine.GetComponent<Animator>();
    }

    public abstract bool IsExecuteOK { get; }
    public abstract void Execute();
    public abstract void FixedUpdate();
    public abstract void ForceStop();
    public void MoveNext() => Current++;
    public abstract StateType Update();
}
