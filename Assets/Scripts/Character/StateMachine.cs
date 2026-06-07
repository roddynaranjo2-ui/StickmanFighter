// StateMachine.cs — FSM genérica para el personaje.
// Garantiza Enter/Exit pareados y tolerancia a CurrentState nulo (defensivo).

using System;

namespace StickmanFighter.Character
{
    public interface IState
    {
        void Enter();
        void Update();
        void FixedUpdate();
        void Exit();
    }

    public sealed class StateMachine
    {
        public IState? CurrentState { get; private set; }

        public void Initialize(IState startState)
        {
            CurrentState = startState ?? throw new ArgumentNullException(nameof(startState));
            CurrentState.Enter();
        }

        public void ChangeState(IState newState)
        {
            if (newState == null) throw new ArgumentNullException(nameof(newState));
            if (ReferenceEquals(newState, CurrentState)) return; // No-op si es el mismo estado
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        public void Update()      => CurrentState?.Update();
        public void FixedUpdate() => CurrentState?.FixedUpdate();
    }
}
