using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    #region public
    public StateType CurrentType;
    [NonSerialized] public Enemy EnemyObj;
    #endregion

    #region private
    Dictionary<StateType, StateBase> _states = new Dictionary<StateType, StateBase>();
    StateBase _curStates;
    #endregion

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        ChangeState(_curStates.Update());
    }

    public bool ChangeState(StateType newStatetype)
    {
        if (CurrentType == newStatetype) 
            return false;

        if (!_states[newStatetype].IsExecuteOK) 
            return false;

        _curStates.ForceStop();
        _curStates = _states[newStatetype];
        _curStates.Execute();
        CurrentType = newStatetype;
        return true;
    }

    void Init()
    {
        for (StateType state = StateType.Idle; state < StateType.EOF; state++)
            AddState(state);

        EnemyObj = gameObject.GetComponent<Enemy>();
        _curStates = _states[StateType.Idle];
        CurrentType = StateType.Idle;
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
