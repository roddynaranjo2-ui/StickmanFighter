// FadeController.cs — fade in/out con un Image negro a pantalla completa.
// FIX: si no hay CanvasGroup, lo añadimos automáticamente.
//      si no hay Image blocker explícita, intentamos auto-localizar la primera Image hija.
// FIX P2-5: setter público SetBlocker() para inyectar la Image desde MainMenuController
//           sin depender del azar de GetComponentInChildren tras refactors futuros.

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace StickmanFighter.UI
{
    public sealed class FadeController : MonoBehaviour
    {
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private Image? _blocker;

        private CanvasGroup _group = null!;

        private void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            if (_group == null) _group = gameObject.AddComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.blocksRaycasts = false;

            if (_blocker == null) _blocker = GetComponentInChildren<Image>(includeInactive: true);
            if (_blocker != null) _blocker.color = new Color(0f, 0f, 0f, 0f);
        }

        /// <summary>FIX P2-5: permite inyectar el Image blocker desde fuera (ej. MainMenuController.EnsureFade)
        /// para no depender de la heurística de GetComponentInChildren. Idempotente.</summary>
        public void SetBlocker(Image blocker)
        {
            if (blocker == null) return;
            _blocker = blocker;
            _blocker.color = new Color(0f, 0f, 0f, _group != null ? _group.alpha : 0f);
        }

        public void FadeIn(Action? onComplete = null)  => StartCoroutine(FadeRoutine(1f, 0f, onComplete));
        public void FadeOut(Action? onComplete = null) => StartCoroutine(FadeRoutine(0f, 1f, onComplete));

        private IEnumerator FadeRoutine(float from, float to, Action? onComplete)
        {
            _group.blocksRaycasts = true;
            float t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Lerp(from, to, t / _fadeDuration);
                _group.alpha = a;
                if (_blocker != null)
                {
                    var c = _blocker.color; c.a = a; _blocker.color = c;
                }
                yield return null;
            }
            _group.alpha = to;
            _group.blocksRaycasts = to > 0.01f;
            onComplete?.Invoke();
        }
    }
}
