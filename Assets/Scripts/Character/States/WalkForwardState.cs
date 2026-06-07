// WalkForwardState.cs — caminar hacia delante (derecha).
// FIX C-09: tratar Forward && Backward == Idle (sin "atasco" de teclas).
// FIX G-04: aplicar flip visual al entrar.

using UnityEngine;

namespace StickmanFighter.Character.States
{
    public sealed class WalkForwardState : PlayerState
    {
        public WalkForwardState(PlayerController player) : base(player) { }

        public override void Enter()
        {
            Player.Facing = 1;
            Player.ApplyFacing();
        }

        public override void Update()
        {
            var input = Player.InputData;
            bool grounded = Player.IsGrounded;

            if (input.JumpPressed && grounded) { Player.StateMachine.ChangeState(Player.JumpState);        return; }
            if (input.Crouch)                  { Player.StateMachine.ChangeState(Player.CrouchState);      return; }
            if (input.PunchPressed)            { Player.StateMachine.ChangeState(Player.PunchAttackState); return; }
            if (input.KickPressed)             { Player.StateMachine.ChangeState(Player.KickAttackState);  return; }

            // FIX C-09: si ambos pulsados o ninguno → Idle. Si solo backward → WalkBackward.
            bool fwd = input.MoveForward;
            bool back = input.MoveBackward;
            if (fwd == back)          { Player.StateMachine.ChangeState(Player.IdleState);         return; }
            if (!fwd && back)         { Player.StateMachine.ChangeState(Player.WalkBackwardState); return; }
            // else: fwd && !back → mantener este estado.
        }

        public override void FixedUpdate()
        {
            Rb.velocity = new Vector2(Player.MoveSpeed, Rb.velocity.y);
        }

        public override void Exit() { }
    }
}
