using Space;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    #region public
    public StateType Current;
    public bool IsMoveable { get; set; }
    #endregion

    #region private
    Dictionary<StateType, StateBase> _states = new Dictionary<StateType, StateBase>();
    StateBase _curStates;
    Enemy enemy;
    #endregion

    private void Awake()
    {
        Init();
        IsMoveable = true;
    }

    private void Update()
    {
        if (IsMoveable) 
        {
            if (enemy.IsMovable)
                ChangeState(StateType.Move);
            else
                ChangeState(StateType.Idle);
        }
    }

    bool ChangeState(StateType newStatetype)
    {
        if (Current == newStatetype) 
            return false;

        if (!_states[newStatetype].IsExecuteOK) 
            return false;

        _curStates.ForceStop();
        _curStates = _states[newStatetype];
        _curStates.Execute();
        Current = newStatetype;
        return true;
    }

    void Init()
    {
        for (StateType state = StateType.Idle; state < StateType.EOF; state++)
            AddState(state);

        enemy = gameObject.GetComponent<Enemy>();
        _curStates = _states[StateType.Idle];
        Current = StateType.Idle;
    }

    void AddState(StateType stateType)
    {
        if (_states.ContainsKey(stateType)) 
            return;

        string typename = $"State{Convert.ToString(stateType)}";
        Debug.Log($"Adding... {typename}");
        Type type = Type.GetType(typename);

        if (type != null) {
            ConstructorInfo constructor = type.GetConstructor(new Type[]
            {
                typeof(StateType),
                typeof(StateMachine)
            });

            StateBase state = constructor.Invoke(new object[2]
            {
                stateType,
                this
            }) as StateBase;

            _states.Add(stateType, state);
            Debug.Log($"Complete...{typename}");
        }
    }
}
