// Hitbox.cs — Caja de daño activable por frames.
// FIX G-02 SPRINT #3: hitboxes para Punch/Kick que se activan/desactivan según ventanas
// definidas por los estados de ataque. Detección por OverlapBox sin requerir Rigidbody en el blanco.

using UnityEngine;

namespace StickmanFighter.Combat
{
    public sealed class Hitbox : MonoBehaviour
    {
        [Header("Hitbox Config")]
        [SerializeField] private Vector2 _offset = new Vector2(0.6f, 0.5f);
        [SerializeField] private Vector2 _size   = new Vector2(0.8f, 0.6f);
        [SerializeField] private int _damage = 10;
        [SerializeField] private LayerMask _targetLayers;
        [SerializeField] private AttackType _attackType = AttackType.Punch;

        [Header("Owner")]
        [Tooltip("Quien ataca. Si null, intenta tomar el HealthSystem del root para evitar friendly fire.")]
        [SerializeField] private HealthSystem? _ownerHealth;

        private bool _active;
        private readonly System.Collections.Generic.HashSet<HealthSystem> _alreadyHit = new();

        public int Damage { get => _damage; set => _damage = value; }
        public AttackType AttackType { get => _attackType; set => _attackType = value; }
        public Vector2 Offset   { get => _offset;   set => _offset   = value; }
        public Vector2 SizeBox  { get => _size;     set => _size     = value; }

        private void Awake()
        {
            if (_ownerHealth == null) _ownerHealth = GetComponentInParent<HealthSystem>();
            // Auto-asignar layer Hitbox si está disponible
            int hitboxLayer = LayerMask.NameToLayer("Hitbox");
            if (hitboxLayer >= 0) gameObject.layer = hitboxLayer;

            // Auto-completar targetLayers si vacía: golpear a Player (8) + Enemy (9) menos a uno mismo
            if (_targetLayers.value == 0)
            {
                int p = LayerMask.NameToLayer("Player");
                int e = LayerMask.NameToLayer("Enemy");
                int mask = 0;
                if (p >= 0) mask |= (1 << p);
                if (e >= 0) mask |= (1 << e);
                _targetLayers = mask;
            }
        }

        /// <summary>Activa la ventana de daño. Llamar al inicio del active-frame del ataque.</summary>
        public void Activate()
        {
            _active = true;
            _alreadyHit.Clear();
        }

        /// <summary>Desactiva la ventana. Llamar al final del active-frame.</summary>
        public void Deactivate()
        {
            _active = false;
            _alreadyHit.Clear();
        }

        private void FixedUpdate()
        {
            if (!_active) return;

            // El offset se aplica respetando el facing del root (localScale.x)
            float facingSign = Mathf.Sign(transform.lossyScale.x);
            Vector2 worldCenter = (Vector2)transform.position + new Vector2(_offset.x * facingSign, _offset.y);

            var hits = Physics2D.OverlapBoxAll(worldCenter, _size, 0f, _targetLayers);
            for (int i = 0; i < hits.Length; i++)
            {
                var col = hits[i];
                var victim = col.GetComponentInParent<HealthSystem>();
                if (victim == null) continue;
                if (victim == _ownerHealth) continue;        // no friendly fire
                if (_alreadyHit.Contains(victim)) continue;  // un hit por activación

                if (victim.ApplyDamage(_damage, transform.position))
                {
                    _alreadyHit.Add(victim);
                    CombatEvents.RaiseHit(transform.position, victim.transform.position, _damage, _attackType);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            float facingSign = Application.isPlaying ? Mathf.Sign(transform.lossyScale.x) : 1f;
            Vector2 worldCenter = (Vector2)transform.position + new Vector2(_offset.x * facingSign, _offset.y);
            Gizmos.color = _active ? new Color(1f, 0.2f, 0.2f, 0.8f) : new Color(1f, 1f, 0f, 0.35f);
            Gizmos.DrawWireCube(worldCenter, _size);
        }
    }
}
