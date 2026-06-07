// CombatEvents.cs — Bus de eventos estático para acoplar VFX/Audio/ScreenShake con Hitbox
// sin que Hitbox dependa de ninguno de ellos. Patrón pub/sub clásico.
// FIX G-02 SPRINT #3.

using System;
using UnityEngine;

namespace StickmanFighter.Combat
{
    public static class CombatEvents
    {
        /// <summary>
        /// Se dispara cada vez que un Hitbox confirma daño sobre un Hurtbox.
        /// Args: (attackerPos, victimPos, damage, attackType)
        /// </summary>
        public static event Action<Vector2, Vector2, int, AttackType>? OnHit;

        public static void RaiseHit(Vector2 attackerPos, Vector2 victimPos, int damage, AttackType type)
        {
            OnHit?.Invoke(attackerPos, victimPos, damage, type);
        }

        /// <summary>Reset (para tests). Limpia subscriptores residuales.</summary>
        public static void ResetForTests() => OnHit = null;
    }

    public enum AttackType { Punch, Kick }
}
