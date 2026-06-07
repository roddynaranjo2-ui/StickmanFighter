// SceneLoader.cs — Singleton para cargar escenas asíncronamente.
// FIX: Awake ya no sobreescribe _instance si el lazy-getter lo creó previamente.
// FIX P2-4: el lazy-getter aplica DontDestroyOnLoad INMEDIATAMENTE para evitar race-condition
//           en la que LoadScene se llamase antes de que Unity dispare el Awake del GO recién creado.

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StickmanFighter.Core
{
    public sealed class SceneLoader : MonoBehaviour
    {
        private static SceneLoader? _instance;
        public static SceneLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[SceneLoader]");
                    _instance = go.AddComponent<SceneLoader>();
                    // FIX P2-4: aplicar DontDestroyOnLoad YA, sin esperar al Awake del frame siguiente.
                    // Sin esto, una llamada inmediata a Instance.LoadScene(...) podría destruir el GO
                    // durante la transición de escena antes de que Awake lo proteja.
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadRoutine(sceneName));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (op == null)
            {
                Debug.LogError($"[SceneLoader] La escena '{sceneName}' no existe en Build Settings.");
                yield break;
            }
            op.allowSceneActivation = true;
            while (!op.isDone) yield return null;
        }
    }
}
