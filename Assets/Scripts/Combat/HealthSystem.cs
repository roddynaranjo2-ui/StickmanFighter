// HealthSystem.cs — Sistema de vida para Player y Enemy.
// FIX G-02 SPRINT #3: introduce HP, damage events, i-frames, muerte y eventos C# nativos
// para que la UI (HealthBar) y los VFX (HitFlash, ScreenShake) se enganchen sin acoplarse.

using System;
using UnityEngine;

namespace StickmanFighter.Combat
{
    public sealed class HealthSystem : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private int _maxHealth = 100;
        [Tooltip("Tiempo de invulnerabilidad tras recibir un hit (segundos)")]
        [SerializeField] private float _invulnerabilityDuration = 0.20f;

        public int MaxHealth => _maxHealth;
        public int CurrentHealth { get; private set; }
        public bool IsAlive => CurrentHealth > 0;
        public bool IsInvulnerable { get; private set; }

        /// <summary>Disparado cuando cambia la vida. (currentHP, maxHP)</summary>
        public event Action<int, int>? OnHealthChanged;
        /// <summary>Disparado al recibir daño. (damage, attackerPos)</summary>
        public event Action<int, Vector2>? OnDamaged;
        /// <summary>Disparado al morir.</summary>
        public event Action? OnDied;

        private float _invulnTimer;

        private void Awake()
        {
            CurrentHealth = _maxHealth;
        }

        private void Update()
        {
            if (IsInvulnerable)
            {
                _invulnTimer -= Time.deltaTime;
                if (_invulnTimer <= 0f) IsInvulnerable = false;
            }
        }

        /// <summary>Aplica daño. Retorna true si el daño se aplicó.</summary>
        public bool ApplyDamage(int amount, Vector2 attackerPos)
        {
            if (!IsAlive || IsInvulnerable || amount <= 0) return false;

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            IsInvulnerable = true;
            _invulnTimer = _invulnerabilityDuration;

            OnDamaged?.Invoke(amount, attackerPos);
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);

            if (CurrentHealth == 0)
            {
                OnDied?.Invoke();
            }
            return true;
        }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0) return;
            CurrentHealth = Mathf.Min(_maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);
        }

        public void ResetHealth()
        {
            CurrentHealth = _maxHealth;
            IsInvulnerable = false;
            _invulnTimer = 0f;
            OnHealthChanged?.Invoke(CurrentHealth, _maxHealth);
        }
    }
}
