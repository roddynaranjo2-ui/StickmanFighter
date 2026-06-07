// HealthBarUI.cs — Barra de vida que se ancla a un HealthSystem (Player o Enemy).
// FIX G-02 SPRINT #3. Crea automáticamente la jerarquía (BG + Fill) si no se asignan.

using UnityEngine;
using UnityEngine.UI;
using StickmanFighter.Combat;

namespace StickmanFighter.UI
{
    public sealed class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private HealthSystem? _target;
        [SerializeField] private Image? _fill;
        [SerializeField] private Color _fullColor = new Color(0.2f, 0.85f, 0.25f);
        [SerializeField] private Color _lowColor  = new Color(0.85f, 0.15f, 0.15f);
        [SerializeField] private float _lerpSpeed = 8f;

        private float _displayed = 1f;
        private float _targetFill = 1f;

        private void OnEnable()
        {
            if (_target != null) _target.OnHealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            if (_target != null) _target.OnHealthChanged -= OnHealthChanged;
        }

        public void Bind(HealthSystem hs)
        {
            if (_target != null) _target.OnHealthChanged -= OnHealthChanged;
            _target = hs;
            if (_target != null)
            {
                _target.OnHealthChanged += OnHealthChanged;
                OnHealthChanged(_target.CurrentHealth, _target.MaxHealth);
            }
        }

        private void OnHealthChanged(int current, int max)
        {
            _targetFill = max > 0 ? (float)current / max : 0f;
        }

        private void Update()
        {
            _displayed = Mathf.Lerp(_displayed, _targetFill, Time.deltaTime * _lerpSpeed);
            if (_fill != null)
            {
                _fill.fillAmount = _displayed;
                _fill.color = Color.Lerp(_lowColor, _fullColor, _displayed);
            }
        }
    }
}
