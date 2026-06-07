// JumpState.cs — salto único, control aéreo al 80%, sin doble salto, sin ataque aéreo en v1.
// FIX G-08: resetear velocity.y a 0 ANTES del AddForce para garantizar altura consistente.
// FIX G-06 (SPRINT #3): SFX Jump al despegar, Land al aterrizar.
// FIX API (SPRINT #3): Rb.velocity (Unity 2022.3 API).

using UnityEngine;
using StickmanFighter.Audio;

namespace StickmanFighter.Character.States
{
    public sealed class JumpState : PlayerState
    {
        public JumpState(PlayerController player) : base(player) { }

        public override void Enter()
        {
            // FIX G-08: anular velocidad vertical previa.
            var v = Rb.velocity;
            v.y = 0f;
            Rb.velocity = v;
            Rb.AddForce(Vector2.up * Player.JumpForce, ForceMode2D.Impulse);
            AudioBus.Play(SfxId.Jump, Player.transform.position);
        }

        public override void Update()
        {
            if (Player.IsGrounded && Rb.velocity.y <= 0.01f)
            {
                AudioBus.Play(SfxId.Land, Player.transform.position);
                var input = Player.InputData;
                if      (input.MoveForward && !input.MoveBackward)  Player.StateMachine.ChangeState(Player.WalkForwardState);
                else if (input.MoveBackward && !input.MoveForward)  Player.StateMachine.ChangeState(Player.WalkBackwardState);
                else                                                Player.StateMachine.ChangeState(Player.IdleState);
            }
        }

        public override void FixedUpdate()
        {
            // Control aéreo al 80% — sólo si una sola dirección está pulsada (resuelve también C-09 en aire).
            bool fwd = Player.InputData.MoveForward;
            bool back = Player.InputData.MoveBackward;
            float dir = (fwd && !back) ? 1f : (back && !fwd) ? -1f : 0f;
            if (dir != 0f)
            {
                var v = Rb.velocity;
                v.x = dir * Player.MoveSpeed * 0.8f;
                Rb.velocity = v;
            }
        }

        public override void Exit() { }
    }
}
