// KickAttackState.cs — ataque patada, duración 0.50s, daño mayor.
// Interrumpible por Jump (si grounded), Crouch, y cancel-into-Punch tras ventana activa (G-05).
//
// FIX SPRINT #3 (v0.1.4):
//  - G-02: activa/desactiva Hitbox del jugador en la ventana activa.
//  - G-06: SFX Kick al iniciar el active-frame.
//  - API: Rb.velocity (Unity 2022.3 API).

using UnityEngine;
using StickmanFighter.Audio;
using StickmanFighter.Combat;

namespace StickmanFighter.Character.States
{
    public sealed class KickAttackState : PlayerState
    {
        private const float Duration    = 0.50f;
        private const float ActiveStart = 0.15f;
        private const float ActiveEnd   = 0.30f;
        private float _timer;
        private bool _hitboxOpen;
        private Hitbox? _hitbox;

        public KickAttackState(PlayerController player) : base(player)
        {
            var hbs = Player.GetComponentsInChildren<Hitbox>(true);
            for (int i = 0; i < hbs.Length; i++)
            {
                if (hbs[i].name == "KickHitbox" || hbs[i].AttackType == AttackType.Kick)
                {
                    _hitbox = hbs[i];
                    break;
                }
            }
        }

        public override void Enter()
        {
            _timer = 0f;
            _hitboxOpen = false;
            var v = Rb.velocity;
            v.x = 0f;
            Rb.velocity = v;
        }

        public override void Update()
        {
            var input = Player.InputData;

            if (input.JumpPressed && Player.IsGrounded)
            {
                CloseHitbox();
                Player.StateMachine.ChangeState(Player.JumpState);
                return;
            }

            if (input.Crouch)
            {
                CloseHitbox();
                Player.StateMachine.ChangeState(Player.CrouchState);
                return;
            }

            // Cancel-into-Punch tras la ventana activa (G-05)
            if (_timer > ActiveEnd && input.PunchPressed)
            {
                CloseHitbox();
                Player.StateMachine.ChangeState(Player.PunchAttackState);
                return;
            }

            _timer += Time.deltaTime;

            if (_timer >= ActiveStart && _timer <= ActiveEnd)
            {
                if (!_hitboxOpen)
                {
                    _hitbox?.Activate();
                    _hitboxOpen = true;
                    AudioBus.Play(SfxId.Kick, Player.transform.position);
                }
            }
            else if (_hitboxOpen)
            {
                CloseHitbox();
            }

            if (_timer >= Duration)
            {
                CloseHitbox();
                Player.StateMachine.ChangeState(Player.IdleState);
            }
        }

        public override void Exit()
        {
            CloseHitbox();
        }

        private void CloseHitbox()
        {
            if (_hitboxOpen)
            {
                _hitbox?.Deactivate();
                _hitboxOpen = false;
            }
        }
    }
}
