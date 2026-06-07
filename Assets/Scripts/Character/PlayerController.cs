// PlayerController.cs — Controlador principal del jugador.
// Posee la FSM, expone parámetros de movimiento, detecta suelo y aplica InputData.
//
// FIX (auditoría v1.1):
//  - GroundCheck por defecto a y=-0.65 / radio 0.20 (M-03 / C-3 unificado a spec prompt v3.0).
//  - Auto-creación de GroundCheck si falta (defensivo).
//  - Auto-asignación del LayerMask "Ground" si está vacío.
//  - Facing público para flip de sprites por estados de Walk (G-04).
//
// FIX SPRINT #3 (v0.1.4):
//  - P1-5: Collider standing 0.6×1.8 / crouch 0.6×0.9 (coincide con sprite real).
//  - P3-4: Parámetros de visual-crouch (HeadCrouchYOffset, BodyCrouchYOffset) ahora serializados aquí
//          (CrouchState los consume vía API pública — sin magic numbers).
//  - G-02: Auto-añade HealthSystem si falta (Player necesita HP para recibir daño del Enemy).
//  - G-02: Expone HealthSystem público para los hitboxes/HUD.

using UnityEngine;
using StickmanFighter.Character.States;
using StickmanFighter.Combat;

namespace StickmanFighter.Character
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpForce = 12f;

        [Header("Ground Detection")]
        [SerializeField] private Transform? _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.20f;
        [SerializeField] private LayerMask _groundLayer;

        [Header("Collider Settings")]
        [SerializeField] private BoxCollider2D? _mainCollider;
        [SerializeField] private Vector2 _standingColliderSize   = new Vector2(0.6f, 1.8f);
        [SerializeField] private Vector2 _standingColliderOffset = new Vector2(0f, 0f);
        [SerializeField] private Vector2 _crouchColliderSize     = new Vector2(0.6f, 0.9f);
        [SerializeField] private Vector2 _crouchColliderOffset   = new Vector2(0f, -0.45f);

        [Header("Visual (FIX P3-4: sin magic numbers en estados)")]
        [SerializeField] private Transform? _bodyTransform;
        [SerializeField] private Transform? _headTransform;
        [Tooltip("Desplazamiento vertical de la cabeza durante crouch (negativo = abajo)")]
        [SerializeField] private float _headCrouchYOffset = -0.35f;
        [Tooltip("Desplazamiento vertical del cuerpo durante crouch (negativo = abajo)")]
        [SerializeField] private float _bodyCrouchYOffset = -0.15f;

        // ────────── Acceso público ──────────
        public float MoveSpeed => _moveSpeed;
        public float JumpForce => _jumpForce;
        public BoxCollider2D MainCollider => _mainCollider!;
        public Vector2 StandingColliderSize   => _standingColliderSize;
        public Vector2 StandingColliderOffset => _standingColliderOffset;
        public Vector2 CrouchColliderSize     => _crouchColliderSize;
        public Vector2 CrouchColliderOffset   => _crouchColliderOffset;

        public Transform? BodyTransform => _bodyTransform;
        public Transform? HeadTransform => _headTransform;
        public float HeadCrouchYOffset => _headCrouchYOffset;
        public float BodyCrouchYOffset => _bodyCrouchYOffset;

        public bool IsGrounded { get; private set; }
        public PlayerInputData InputData { get; set; }
        public Rigidbody2D Rb { get; private set; } = null!;

        /// <summary>+1 = mira a la derecha, -1 = mira a la izquierda. Lo controlan los estados de Walk.</summary>
        public int Facing { get; set; } = 1;

        /// <summary>Sistema de vida (FIX G-02). Auto-añadido si falta.</summary>
        public HealthSystem Health { get; private set; } = null!;

        public StateMachine StateMachine { get; private set; } = null!;
        public IdleState         IdleState         { get; private set; } = null!;
        public WalkForwardState  WalkForwardState  { get; private set; } = null!;
        public WalkBackwardState WalkBackwardState { get; private set; } = null!;
        public CrouchState       CrouchState       { get; private set; } = null!;
        public JumpState         JumpState         { get; private set; } = null!;
        public PunchAttackState  PunchAttackState  { get; private set; } = null!;
        public KickAttackState   KickAttackState   { get; private set; } = null!;

        private void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            Rb.gravityScale = 3f;
            Rb.constraints  = RigidbodyConstraints2D.FreezeRotation;
            Rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            Rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (_mainCollider == null) _mainCollider = GetComponent<BoxCollider2D>();

            // FIX defensivo: asegurar tag Player
            if (gameObject.tag == "Untagged")
            {
                try { gameObject.tag = "Player"; }
                catch { /* TagManager.asset no contiene "Player" — fallback silencioso */ }
            }

            // FIX P1-1: asegurar layer Player (8) si está en Default
            if (gameObject.layer == 0)
            {
                int playerLayer = LayerMask.NameToLayer("Player");
                if (playerLayer >= 0) gameObject.layer = playerLayer;
            }

            // FIX C-08 defensivo: si el LayerMask está vacío, auto-asigna la layer "Ground" (index 7)
            if (_groundLayer.value == 0)
            {
                int groundLayerIndex = LayerMask.NameToLayer("Ground");
                if (groundLayerIndex >= 0) _groundLayer = 1 << groundLayerIndex;
            }

            // FIX defensivo: auto-crear GroundCheck si falta
            if (_groundCheck == null)
            {
                var existing = transform.Find("GroundCheck");
                if (existing != null)
                {
                    _groundCheck = existing;
                }
                else
                {
                    var go = new GameObject("GroundCheck");
                    go.transform.SetParent(transform, worldPositionStays: false);
                    go.transform.localPosition = new Vector3(0f, -0.9f, 0f);
                    _groundCheck = go.transform;
                }
            }

            // FIX defensivo: auto-cablear Body/Head transforms para crouch visual
            if (_bodyTransform == null) _bodyTransform = transform.Find("Body");
            if (_headTransform == null) _headTransform = transform.Find("Head");

            // FIX G-02: HealthSystem auto-añadido si falta
            Health = GetComponent<HealthSystem>();
            if (Health == null) Health = gameObject.AddComponent<HealthSystem>();

            // Instanciación de estados
            IdleState         = new IdleState(this);
            WalkForwardState  = new WalkForwardState(this);
            WalkBackwardState = new WalkBackwardState(this);
            CrouchState       = new CrouchState(this);
            JumpState         = new JumpState(this);
            PunchAttackState  = new PunchAttackState(this);
            KickAttackState   = new KickAttackState(this);

            StateMachine = new StateMachine();
            StateMachine.Initialize(IdleState);
        }

        private void Update()
        {
            CheckGrounded();
            StateMachine.Update();
        }

        private void FixedUpdate() => StateMachine.FixedUpdate();

        private void CheckGrounded()
        {
            if (_groundCheck == null) { IsGrounded = false; return; }
            IsGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
        }

        /// <summary>Aplica el flip visual a los sprites hijos (G-04).</summary>
        public void ApplyFacing()
        {
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (Facing >= 0 ? 1f : -1f);
            transform.localScale = s;
        }

        private void OnDrawGizmosSelected()
        {
            if (_groundCheck == null) return;
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
        }
    }
}
