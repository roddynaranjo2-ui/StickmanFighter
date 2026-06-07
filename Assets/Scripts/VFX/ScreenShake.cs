// ScreenShake.cs — Sacude la cámara cuando se produce un hit. Suscrito al CombatEvents.
// FIX G-07 SPRINT #3.

using System.Collections;
using UnityEngine;
using StickmanFighter.Combat;

namespace StickmanFighter.VFX
{
    public sealed class ScreenShake : MonoBehaviour
    {
        [SerializeField] private float _shakeDuration = 0.12f;
        [SerializeField] private float _shakeAmplitude = 0.10f;
        [SerializeField] private float _shakeFrequency = 35f;

        private Vector3 _baseLocalPos;
        private Coroutine? _running;

        private void Awake()
        {
            _baseLocalPos = transform.localPosition;
        }

        private void OnEnable()
        {
            CombatEvents.OnHit += HandleHit;
        }

        private void OnDisable()
        {
            CombatEvents.OnHit -= HandleHit;
        }

        private void HandleHit(Vector2 attackerPos, Vector2 victimPos, int damage, AttackType type)
        {
            float intensity = (type == AttackType.Kick) ? 1.4f : 1f;
            if (_running != null) StopCoroutine(_running);
            _running = StartCoroutine(ShakeRoutine(intensity));
        }

        private IEnumerator ShakeRoutine(float intensity)
        {
            float t = 0f;
            while (t < _shakeDuration)
            {
                t += Time.deltaTime;
                float falloff = 1f - (t / _shakeDuration);
                float dx = Mathf.Sin(Time.time * _shakeFrequency) * _shakeAmplitude * intensity * falloff;
                float dy = Mathf.Cos(Time.time * _shakeFrequency * 1.13f) * _shakeAmplitude * intensity * falloff;
                transform.localPosition = _baseLocalPos + new Vector3(dx, dy, 0f);
                yield return null;
            }
            transform.localPosition = _baseLocalPos;
            _running = null;
        }
    }
}
