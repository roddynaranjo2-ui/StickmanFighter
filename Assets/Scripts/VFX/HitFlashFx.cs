// HitFlashFx.cs — Cuando un Hitbox detecta impacto, parpadea el sprite root con color rojo/blanco.
// Acoplado al CombatEvents (pub/sub) → sin dependencias hacia Hitbox.
// FIX G-07 SPRINT #3.

using System.Collections;
using UnityEngine;
using StickmanFighter.Combat;

namespace StickmanFighter.VFX
{
    public sealed class HitFlashFx : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer[] _renderers = System.Array.Empty<SpriteRenderer>();
        [SerializeField] private Color _flashColor = Color.red;
        [SerializeField] private float _flashDuration = 0.10f;
        [SerializeField] private HealthSystem? _watchedHealth;

        private Color[] _originalColors = System.Array.Empty<Color>();
        private Coroutine? _running;

        private void Awake()
        {
            if (_renderers == null || _renderers.Length == 0)
                _renderers = GetComponentsInChildren<SpriteRenderer>(true);

            _originalColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
                _originalColors[i] = _renderers[i].color;

            if (_watchedHealth == null) _watchedHealth = GetComponentInParent<HealthSystem>();
        }

        private void OnEnable()
        {
            if (_watchedHealth != null) _watchedHealth.OnDamaged += HandleDamaged;
        }

        private void OnDisable()
        {
            if (_watchedHealth != null) _watchedHealth.OnDamaged -= HandleDamaged;
        }

        private void HandleDamaged(int dmg, Vector2 attackerPos)
        {
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            for (int i = 0; i < _renderers.Length; i++) _renderers[i].color = _flashColor;
            yield return new WaitForSeconds(_flashDuration);
            for (int i = 0; i < _renderers.Length; i++) _renderers[i].color = _originalColors[i];
            _running = null;
        }
    }
}
