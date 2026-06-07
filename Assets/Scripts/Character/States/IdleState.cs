// IdleState.cs — estado en reposo. Evalúa transiciones según tabla 4.3.

using UnityEngine;

namespace StickmanFighter.Character.States
{
    public sealed class IdleState : PlayerState
    {
        public IdleState(PlayerController player) : base(player) { }

        public override void Enter()
        {
            Rb.velocity = new Vector2(0f, Rb.velocity.y);
        }

        public override void Update()
        {
            var input = Player.InputData;
            bool grounded = Player.IsGrounded;

            // 1. Interrupciones críticas
            if (input.JumpPressed && grounded) { Player.StateMachine.ChangeState(Player.JumpState); return; }
            if (input.Crouch && grounded)      { Player.StateMachine.ChangeState(Player.CrouchState); return; }

            // 2. Ataques
            if (input.PunchPressed && grounded) { Player.StateMachine.ChangeState(Player.PunchAttackState); return; }
            if (input.KickPressed  && grounded) { Player.StateMachine.ChangeState(Player.KickAttackState);  return; }

            // 3. Movimiento
            if (input.MoveForward)  { Player.StateMachine.ChangeState(Player.WalkForwardState);  return; }
            if (input.MoveBackward) { Player.StateMachine.ChangeState(Player.WalkBackwardState); return; }
        }

        public override void Exit() { }
    }
}
