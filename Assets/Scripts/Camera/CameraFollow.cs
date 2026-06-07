// CameraFollow.cs — seguimiento suave del jugador con SmoothDamp.
// X libre, Y clampeada entre _minY y _maxY. Z fijo en _offset.z.
// FIX C-05: auto-cableo de _target con FindWithTag("Player") en Awake.

using UnityEngine;

namespace StickmanFighter.CameraSystem
{
    public sealed class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform? _target;

        [Header("Follow Settings")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 1.5f, -10f);
        [SerializeField] private float _smoothTimeX = 0.15f;
        [SerializeField] private float _smoothTimeY = 0.30f;

        [Header("Vertical Bounds (X is unbounded)")]
        [SerializeField] private float _minY = -5f;
        [SerializeField] private float _maxY = 10f;

        private float _velocityX;
        private float _velocityY;

        public void SetTarget(Transform target) => _target = target;

        private void Awake()
        {
            // FIX C-05: auto-cableo defensivo si _target == null.
            TryAutoBindTarget();
        }

        private void Start()
        {
            // Reintenta en Start por si el Player se instancia más tarde.
            if (_target == null) TryAutoBindTarget();

            // Posiciona la cámara inmediatamente sobre el target para evitar 1 frame visible en (0,0).
            if (_target != null)
            {
                float x = _target.position.x + _offset.x;
                float y = Mathf.Clamp(_target.position.y + _offset.y, _minY, _maxY);
                transform.position = new Vector3(x, y, _offset.z);
            }
        }

        private void TryAutoBindTarget()
        {
            if (_target != null) return;
            GameObject? go = null;
            try { go = GameObject.FindWithTag("Player"); }
            catch { go = null; /* tag no definida */ }

            if (go == null)
            {
                // Fallback por tipo (si el tag no estuviera registrado en TagManager).
                var pc = Object.FindAnyObjectByType<StickmanFighter.Character.PlayerController>();
                if (pc != null) go = pc.gameObject;
            }
            if (go != null) _target = go.transform;
        }

        private void LateUpdate()
        {
            if (_target == null) { TryAutoBindTarget(); return; }

            float targetX = _target.position.x + _offset.x;
            float targetY = Mathf.Clamp(_target.position.y + _offset.y, _minY, _maxY);

            float newX = Mathf.SmoothDamp(transform.position.x, targetX, ref _velocityX, _smoothTimeX);
            float newY = Mathf.SmoothDamp(transform.position.y, targetY, ref _velocityY, _smoothTimeY);

            transform.position = new Vector3(newX, newY, _offset.z);
        }
    }
}
