﻿using System.Collections.Generic;

public abstract class BaseState
{
    public virtual void onInit(params object[] args) {}
    public virtual void onUpdate(float deltaTime) {}
    public virtual void onFixedUpdate(float deltaTime) {}
    public virtual void onExit() {}
}

public class EmptyState : BaseState
{
}

public class StateMachine<T>
{
    public T CurrentState { get; private set; }
    public T PreviousState { get; private set; }
    public bool AlreadyAdded(T s) { return _states.ContainsKey(s); }

    private Dictionary<T, BaseState> _states = new Dictionary<T, BaseState>();
    private BaseState _currentState = new EmptyState();

    public void AddState(T id, BaseState state)
    {
        _states.Add(id, state);
    }

    public void ChangeState(T id, params object[] args)
    {
        PreviousState = CurrentState;
        CurrentState = id;
        _currentState.onExit();
        _currentState = _states[id];
        _currentState.onInit(args);
    }

    public void OnUpdate(float deltaTime)
    {
        _currentState.onUpdate(deltaTime);
    }

    public void OnFixedUpdate(float deltaTime)
    {
        _currentState.onFixedUpdate(deltaTime);
    }
}
