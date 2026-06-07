// PunchAttackState.cs — ataque puñetazo, duración 0.35s.
// Interrumpible por Jump (si grounded), Crouch, y por Kick (G-05: cancel-into-Kick).
//
// FIX SPRINT #3 (v0.1.4):
//  - G-02: activa/desactiva Hitbox del jugador en la ventana activa (frames 6-9 aprox).
//  - G-06: SFX Punch al iniciar el active-frame.
//  - G-05: cancel-into-Kick permitido tras pasar la ventana activa.
//  - API: Rb.velocity (Unity 2022.3 API).

using UnityEngine;
using StickmanFighter.Audio;
using StickmanFighter.Combat;

namespace StickmanFighter.Character.States
{
    public sealed class PunchAttackState : PlayerState
    {
        private const float Duration      = 0.35f;
        private const float ActiveStart   = 0.10f;
        private const float ActiveEnd     = 0.20f;
        private float _timer;
        private bool _hitboxOpen;
        private Hitbox? _hitbox;

        public PunchAttackState(PlayerController player) : base(player)
        {
            // Buscar hitbox "PunchHitbox" entre los hijos
            var hbs = Player.GetComponentsInChildren<Hitbox>(true);
            for (int i = 0; i < hbs.Length; i++)
            {
                if (hbs[i].name == "PunchHitbox" || hbs[i].AttackType == AttackType.Punch)
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

            // 1. Interrupción Jump
            if (input.JumpPressed && Player.IsGrounded)
            {
                CloseHitbox();
                Player.StateMachine.ChangeState(Player.JumpState);
                return;
            }

            // 2. Interrupción Crouch
            if (input.Crouch)
            {
                CloseHitbox();
                Player.StateMachine.ChangeState(Player.CrouchState);
                return;
            }

            // 3. Cancel-into-Kick tras la ventana activa (G-05)
            if (_timer > ActiveEnd && input.KickPressed)
            {
                CloseHitbox();
                Player.StateMachine.ChangeState(Player.KickAttackState);
                return;
            }

            _timer += Time.deltaTime;

            // Ventana activa de daño
            if (_timer >= ActiveStart && _timer <= ActiveEnd)
            {
                if (!_hitboxOpen)
                {
                    _hitbox?.Activate();
                    _hitboxOpen = true;
                    AudioBus.Play(SfxId.Punch, Player.transform.position);
                }
            }
            else if (_hitboxOpen)
            {
                CloseHitbox();
            }

            // 4. Timeout natural
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
