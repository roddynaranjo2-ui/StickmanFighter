// CrouchState.cs — agacharse. Bloquea Move/Jump/Punch/Kick.
// Única salida: Crouch == false → IdleState.
// FIX G-03: ahora también reposiciona la Head y Body para que el sprite se vea agachado.
// FIX P3-4 (SPRINT #3): magic numbers removidos. Los offsets se leen desde PlayerController.

using UnityEngine;

namespace StickmanFighter.Character.States
{
    public sealed class CrouchState : PlayerState
    {
        private Vector3? _headOriginalPos;
        private Vector3? _bodyOriginalPos;

        public CrouchState(PlayerController player) : base(player) { }

        public override void Enter()
        {
            // Reducir collider
            if (Player.MainCollider != null)
            {
                Player.MainCollider.size   = Player.CrouchColliderSize;
                Player.MainCollider.offset = Player.CrouchColliderOffset;
            }

            // FIX G-03 + P3-4: Reposicionar Head/Body usando offsets parametrizados
            if (Player.HeadTransform != null)
            {
                _headOriginalPos = Player.HeadTransform.localPosition;
                var p = _headOriginalPos.Value;
                p.y += Player.HeadCrouchYOffset;
                Player.HeadTransform.localPosition = p;
            }
            if (Player.BodyTransform != null)
            {
                _bodyOriginalPos = Player.BodyTransform.localPosition;
                var p = _bodyOriginalPos.Value;
                p.y += Player.BodyCrouchYOffset;
                Player.BodyTransform.localPosition = p;
            }

            // Detener movimiento horizontal
            var v = Player.Rb.velocity;
            v.x = 0f;
            Player.Rb.velocity = v;
        }

        public override void Update()
        {
            if (!Player.InputData.Crouch)
            {
                Player.StateMachine.ChangeState(Player.IdleState);
            }
        }

        public override void Exit()
        {
            // Restaurar collider de pie
            if (Player.MainCollider != null)
            {
                Player.MainCollider.size   = Player.StandingColliderSize;
                Player.MainCollider.offset = Player.StandingColliderOffset;
            }

            // Restaurar posiciones originales
            if (Player.HeadTransform != null && _headOriginalPos.HasValue)
            {
                Player.HeadTransform.localPosition = _headOriginalPos.Value;
                _headOriginalPos = null;
            }
            if (Player.BodyTransform != null && _bodyOriginalPos.HasValue)
            {
                Player.BodyTransform.localPosition = _bodyOriginalPos.Value;
                _bodyOriginalPos = null;
            }
        }
    }
}
