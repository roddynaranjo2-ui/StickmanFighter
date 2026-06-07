// EnemyController.cs — IA simple para un enemigo stickman.
// FIX G-02 SPRINT #3: completa el ciclo de gameplay añadiendo un oponente real.
//
// FSM minimalista:
//   Idle → (player en rango medio) → Approach
//   Approach → (player en rango ataque) → Attack
//   Attack → cooldown → Idle
//   * → (HP==0) → Dead

using UnityEngine;
using StickmanFighter.Character;
using StickmanFighter.Combat;
using StickmanFighter.Audio;

namespace StickmanFighter.Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class EnemyController : MonoBehaviour
    {
        public enum AIState { Idle, Approach, Attack, Cooldown, Dead }

        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 2.5f;

        [Header("AI Ranges")]
        [SerializeField] private float _detectRange = 8f;
        [SerializeField] private float _attackRange = 1.2f;
        [SerializeField] private float _attackCooldown = 1.2f;
        [SerializeField] private float _attackWindup = 0.25f;
        [SerializeField] private float _attackActive = 0.15f;

        [Header("Combat")]
        [SerializeField] private int _damage = 8;
        [SerializeField] private Hitbox? _hitbox;

        [Header("Target")]
        [SerializeField] private Transform? _target;

        public AIState CurrentState { get; private set; } = AIState.Idle;
        public HealthSystem Health { get; private set; } = null!;

        private Rigidbody2D _rb = null!;
        private float _timer;
        private int _facing = -1;
        private bool _hitboxIsOpen;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 3f;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            Health = GetComponent<HealthSystem>();
            if (Health == null) Health = gameObject.AddComponent<HealthSystem>();
            Health.OnDied += OnDied;

            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0) gameObject.layer = enemyLayer;
            if (gameObject.tag == "Untagged")
            {
                try { gameObject.tag = "Enemy"; } catch { }
            }

            if (_hitbox == null) _hitbox = GetComponentInChildren<Hitbox>(true);
            if (_hitbox != null)
            {
                _hitbox.Damage = _damage;
                _hitbox.Deactivate();
            }

            if (_target == null)
            {
                var player = FindFirstObjectByType<PlayerController>();
                if (player != null) _target = player.transform;
            }
        }

        private void OnDestroy()
        {
            if (Health != null) Health.OnDied -= OnDied;
        }

        private void Update()
        {
            if (CurrentState == AIState.Dead || _target == null) return;

            float dx = _target.position.x - transform.position.x;
            float dist = Mathf.Abs(dx);
            _facing = dx >= 0 ? 1 : -1;

            // Aplicar flip al sprite
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * _facing;
            transform.localScale = s;

            switch (CurrentState)
            {
                case AIState.Idle:
                    if (dist < _detectRange) CurrentState = AIState.Approach;
                    break;

                case AIState.Approach:
                    if (dist <= _attackRange) { EnterAttack(); }
                    break;

                case AIState.Attack:
                    _timer -= Time.deltaTime;
                    // Activar hitbox en la ventana activa
                    float elapsed = (_attackWindup + _attackActive) - _timer;
                    if (_hitbox != null)
                    {
                        bool inActiveWindow = elapsed >= _attackWindup && elapsed <= _attackWindup + _attackActive;
                        if (inActiveWindow && !_hitboxIsOpen) { _hitbox.Activate(); _hitboxIsOpen = true; AudioBus.Play(SfxId.Punch, transform.position); }
                        else if (!inActiveWindow && _hitboxIsOpen) { _hitbox.Deactivate(); _hitboxIsOpen = false; }
                    }
                    if (_timer <= 0f)
                    {
                        if (_hitbox != null) { _hitbox.Deactivate(); _hitboxIsOpen = false; }
                        _timer = _attackCooldown;
                        CurrentState = AIState.Cooldown;
                    }
                    break;

                case AIState.Cooldown:
                    _timer -= Time.deltaTime;
                    if (_timer <= 0f)
                        CurrentState = (dist <= _attackRange) ? Begin(AIState.Attack, _attackWindup + _attackActive)
                                                              : AIState.Approach;
                    break;
            }
        }

        private AIState Begin(AIState s, float t) { _timer = t; return s; }

        private void EnterAttack()
        {
            CurrentState = AIState.Attack;
            _timer = _attackWindup + _attackActive;
            _hitboxIsOpen = false;
        }

        private void FixedUpdate()
        {
            if (CurrentState != AIState.Approach || _target == null) return;
            float vx = _moveSpeed * _facing;
            _rb.velocity = new Vector2(vx, _rb.velocity.y);
        }

        private void OnDied()
        {
            CurrentState = AIState.Dead;
            _rb.velocity = Vector2.zero;
            if (_hitbox != null) _hitbox.Deactivate();
            AudioBus.Play(SfxId.Death, transform.position);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;  Gizmos.DrawWireSphere(transform.position, _detectRange);
            Gizmos.color = Color.red;     Gizmos.DrawWireSphere(transform.position, _attackRange);
        }
    }
}
