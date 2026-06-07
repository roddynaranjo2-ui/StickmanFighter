// MainMenuController.cs — Controla los botones del menú principal.
//
// FIX C-03 + C-04: si los botones no están cableados o el Canvas no tiene los componentes correctos,
// CONSTRUIMOS el Canvas + Title + PlayButton + QuitButton + FadeBlocker por código en Awake.
// Resultado: el menú es funcional incluso con un GameObject "Canvas" vacío en la escena.
// FIX P2-5: EnsureFade() ahora inyecta el Image blocker explícitamente vía SetBlocker().

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using StickmanFighter.Core;

namespace StickmanFighter.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button? _playButton;
        [SerializeField] private Button? _quitButton;
        [SerializeField] private FadeController? _fade;

        private void Awake()
        {
            EnsureEventSystem();
            EnsureCanvasAndButtons();
            EnsureFade();

            if (_playButton != null) _playButton.onClick.AddListener(OnPlay);
            if (_quitButton != null) _quitButton.onClick.AddListener(OnQuit);
        }

        private void OnDestroy()
        {
            if (_playButton != null) _playButton.onClick.RemoveListener(OnPlay);
            if (_quitButton != null) _quitButton.onClick.RemoveListener(OnQuit);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private void EnsureCanvasAndButtons()
        {
            // Si los botones ya están cableados desde el inspector, no construimos nada.
            if (_playButton != null && _quitButton != null) return;

            // Buscar Canvas existente o crear uno nuevo.
            var canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                // Si este GO ("Canvas" de la escena) NO tiene Canvas component → lo añadimos (FIX C-04).
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 0;
            }
            if (GetComponent<CanvasScaler>() == null)
            {
                var scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            // Asegurar que el RectTransform cubra toda la pantalla.
            var rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            // Fondo
            CreateBackground(transform);

            // Título
            CreateTitle(transform, "STICKMAN FIGHTER");

            // Botones
            if (_playButton == null) _playButton = CreateMenuButton(transform, "PlayButton", "PLAY",  new Vector2(0.5f, 0.5f), new Vector2(0f, -30f));
            if (_quitButton == null) _quitButton = CreateMenuButton(transform, "QuitButton", "QUIT", new Vector2(0.5f, 0.5f), new Vector2(0f, -180f));
        }

        private void EnsureFade()
        {
            if (_fade != null) return;

            // Crear un GO FadeBlocker hijo del Canvas con CanvasGroup + Image negro a pantalla completa + FadeController.
            var go = new GameObject("FadeBlocker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            go.transform.SetParent(transform, worldPositionStays: false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.SetAsLastSibling();

            var img = go.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f);
            img.raycastTarget = false;   // No bloquea clicks cuando es transparente

            _fade = go.AddComponent<FadeController>();
            // FIX P2-5: inyectamos el Image blocker explícitamente. Antes el FadeController dependía de
            // GetComponentInChildren<Image>() para encontrarlo — funcionaba por azar (es el mismo GO).
            _fade.SetBlocker(img);
        }

        private static void CreateBackground(Transform parent)
        {
            var go = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, worldPositionStays: false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.SetAsFirstSibling();
            var img = go.GetComponent<Image>();
            img.color = new Color(0.10f, 0.12f, 0.18f, 1f);
        }

        private static void CreateTitle(Transform parent, string label)
        {
            var go = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, worldPositionStays: false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0f, 250f);
            rt.sizeDelta = new Vector2(1200f, 200f);
            var txt = go.GetComponent<Text>();
            txt.text = label;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 96;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static Button CreateMenuButton(Transform parent, string name, string label, Vector2 anchor, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, worldPositionStays: false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(420f, 110f);
            var img = go.GetComponent<Image>();
            img.color = new Color(0.20f, 0.55f, 0.85f, 1f);

            // Label
            var txtGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            txtGo.transform.SetParent(go.transform, worldPositionStays: false);
            var trt = (RectTransform)txtGo.transform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            var txt = txtGo.GetComponent<Text>();
            txt.text = label;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 56;
            txt.fontStyle = FontStyle.Bold;
            txt.color = Color.white;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return go.GetComponent<Button>();
        }

        private void OnPlay()
        {
            if (_fade != null)
            {
                _fade.FadeOut(() => SceneLoader.Instance.LoadScene("CombatScene"));
            }
            else
            {
                SceneLoader.Instance.LoadScene("CombatScene");
            }
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
