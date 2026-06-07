// MobileInputHandler.cs — Mezcla input táctil (botones UI) + teclado (legacy Input.GetAxis).
// Ejecuta en ScriptExecutionOrder = -100 para correr ANTES de PlayerController.
//
// FIX C-01: auto-cableo de _player con FindWithTag("Player").
// FIX G-01: si no hay Canvas táctil en la escena, lo construye por código (6 botones HUD).
// FIX G-07: edge-flags se resetean en OnDisable y al perder foco para evitar inputs fantasma.
// FIX P2-1: eliminada la doble escritura redundante de _player.InputData en LateUpdate.
// FIX P2-2: los botones de ataque (Punch/Kick/Jump) usan EventTrigger.PointerDown en vez de Button.onClick
//           (onClick dispara en PointerUp y añadía ~80-150 ms de latencia en móvil).

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using StickmanFighter.Character;
using StickmanFighter.Core;

namespace StickmanFighter.UI
{
    public sealed class MobileInputHandler : MonoBehaviour
    {
        [SerializeField] private PlayerController? _player;
        [SerializeField] private bool _autoBuildTouchHud = true;

        private PlayerInputData _inputData;

        // Estado táctil persistente — se mantiene entre frames mediante PointerDown/Up.
        private bool _touchMoveForward;
        private bool _touchMoveBackward;
        private bool _touchCrouch;

        // Pausa in-game construida por código en CombatScene.
        private GameObject? _pauseOverlay;
        private CanvasGroup? _pauseOverlayGroup;

        private static Sprite? _hudButtonSprite;

        public void SetPlayer(PlayerController player) => _player = player;

        // ─────── Métodos públicos invocados por EventTrigger en los botones UI ───────
        public void OnMoveForwardDown()  { _touchMoveForward  = true;  }
        public void OnMoveForwardUp()    { _touchMoveForward  = false; }
        public void OnMoveBackwardDown() { _touchMoveBackward = true;  }
        public void OnMoveBackwardUp()   { _touchMoveBackward = false; }
        public void OnCrouchDown()       { _touchCrouch       = true;  }
        public void OnCrouchUp()         { _touchCrouch       = false; }
        public void OnJumpDown()         { _inputData.JumpPressed  = true; }
        public void OnPunchDown()        { _inputData.PunchPressed = true; }
        public void OnKickDown()         { _inputData.KickPressed  = true; }

        private bool IsGamePaused() => GameManager.Instance != null &&
                                       GameManager.Instance.CurrentState == GameManager.GameState.Paused;

        private void TogglePause()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            if (gm.CurrentState == GameManager.GameState.Paused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        private void PauseGame()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.PauseGame();
            SetPauseOverlayVisible(true);
        }

        private void ResumeGame()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.ResumeGame();
            SetPauseOverlayVisible(false);
        }

        private void QuitToMenu()
        {
            var gm = GameManager.Instance;
            if (gm != null) gm.ResumeGame();
            SetPauseOverlayVisible(false);
            SceneLoader.Instance.LoadScene("MainMenu");
        }

        private void SetPauseOverlayVisible(bool visible)
        {
            if (_pauseOverlay == null || _pauseOverlayGroup == null) return;
            _pauseOverlay.SetActive(visible);
            _pauseOverlayGroup.alpha = visible ? 1f : 0f;
            _pauseOverlayGroup.interactable = visible;
            _pauseOverlayGroup.blocksRaycasts = visible;
        }

        private void Awake()
        {
            // FIX C-01: auto-cableo defensivo.
            TryAutoBindPlayer();
        }

        private void Start()
        {
            if (_player == null) TryAutoBindPlayer();

            // FIX G-01: garantizar EventSystem y construir HUD táctil si no existe.
            EnsureEventSystem();
            if (_autoBuildTouchHud && !TouchHudExists()) BuildTouchHud();
        }

        private void TryAutoBindPlayer()
        {
            if (_player != null) return;
            try
            {
                var go = GameObject.FindWithTag("Player");
                if (go != null) _player = go.GetComponent<PlayerController>();
            }
            catch { /* tag no registrada */ }
            if (_player == null) _player = Object.FindAnyObjectByType<PlayerController>();
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        private static bool TouchHudExists()
        {
            return GameObject.Find("TouchHud_Canvas") != null;
        }

        // ─────── Auto-build del HUD táctil ───────
        private void BuildTouchHud()
        {
            // Canvas
            var canvasGo = new GameObject("TouchHud_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Botones (siempre anclados a los bordes — soporta rotación landscape ↔ landscape).
            //
            //  IZQ:  [◀] [▶] [▼]      DCH:  [✊]  [🦵]  [↑]
            //
            const float btnSize = 180f;
            const float pad = 30f;

            CreateHoldButton(canvas.transform, "MoveBackBtn",    "◀",  new Vector2(0,0), new Vector2(0,0), new Vector2(pad + btnSize*0.5f,             pad + btnSize*0.5f), btnSize,
                () => _touchMoveBackward = true,  () => _touchMoveBackward = false);
            CreateHoldButton(canvas.transform, "MoveFwdBtn",     "▶",  new Vector2(0,0), new Vector2(0,0), new Vector2(pad + btnSize*1.5f + pad,       pad + btnSize*0.5f), btnSize,
                () => _touchMoveForward  = true,  () => _touchMoveForward  = false);
            CreateHoldButton(canvas.transform, "CrouchBtn",      "▼",  new Vector2(0,0), new Vector2(0,0), new Vector2(pad + btnSize*0.5f,             pad + btnSize*1.5f + pad), btnSize,
                () => _touchCrouch       = true,  () => _touchCrouch       = false);

            CreatePressButton(canvas.transform, "PunchBtn", "P",  new Vector2(1,0), new Vector2(1,0), new Vector2(-(pad + btnSize*1.5f + pad), pad + btnSize*0.5f), btnSize, () => _inputData.PunchPressed = true);
            CreatePressButton(canvas.transform, "KickBtn",  "K",  new Vector2(1,0), new Vector2(1,0), new Vector2(-(pad + btnSize*0.5f),             pad + btnSize*0.5f), btnSize, () => _inputData.KickPressed  = true);
            CreatePressButton(canvas.transform, "JumpBtn",  "↑",  new Vector2(1,0), new Vector2(1,0), new Vector2(-(pad + btnSize*1.0f),             pad + btnSize*1.5f + pad), btnSize, () => _inputData.JumpPressed  = true);

            // Botón de pausa visible (P1-6): abre un overlay con Resume / Menu.
            CreatePressButton(canvas.transform, "PauseBtn", "II", new Vector2(1,1), new Vector2(1,1), new Vector2(-(pad + 60f), -(pad + 60f)), 120f, TogglePause);
            BuildPauseOverlay(canvas.transform);
        }

        private static Sprite? LoadHudButtonSprite()
        {
            if (_hudButtonSprite != null) return _hudButtonSprite;
            _hudButtonSprite = Resources.Load<Sprite>("Sprites/btn_circle");
            return _hudButtonSprite;
        }

        private static GameObject CreateButtonRoot(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, float size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, worldPositionStays: false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(size, size);

            var img = go.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.30f);
            var hudSprite = LoadHudButtonSprite();
            if (hudSprite != null)
            {
                img.sprite = hudSprite;
                img.preserveAspect = true;
            }

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
            txt.fontSize = (int)(size * 0.45f);
            txt.color = Color.white;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return go;
        }

        private static void CreateHoldButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, float size,
            Action onDown, Action onUp)
        {
            var go = CreateButtonRoot(parent, name, label, anchorMin, anchorMax, anchoredPos, size);
            var trig = go.AddComponent<EventTrigger>();

            var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            entryDown.callback.AddListener(_ => onDown());
            trig.triggers.Add(entryDown);

            var entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            entryUp.callback.AddListener(_ => onUp());
            trig.triggers.Add(entryUp);

            var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            entryExit.callback.AddListener(_ => onUp());
            trig.triggers.Add(entryExit);
        }

        private static void CreatePressButton(Transform parent, string name, string label,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, float size,
            Action onPress)
        {
            // FIX P2-2: usar EventTrigger.PointerDown en vez de Button.onClick (que dispara en PointerUp).
            // Esto elimina la latencia de ~80-150 ms percibida al pulsar los botones de ataque/salto.
            var go = CreateButtonRoot(parent, name, label, anchorMin, anchorMax, anchoredPos, size);
            var trig = go.AddComponent<EventTrigger>();
            var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            entryDown.callback.AddListener(_ => onPress());
            trig.triggers.Add(entryDown);
        }

        private void BuildPauseOverlay(Transform parent)
        {
            _pauseOverlay = new GameObject("PauseOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            _pauseOverlay.transform.SetParent(parent, worldPositionStays: false);

            var rt = (RectTransform)_pauseOverlay.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var bg = _pauseOverlay.GetComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.72f);

            _pauseOverlayGroup = _pauseOverlay.GetComponent<CanvasGroup>();
            _pauseOverlayGroup.alpha = 0f;
            _pauseOverlayGroup.interactable = false;
            _pauseOverlayGroup.blocksRaycasts = false;
            _pauseOverlay.SetActive(false);

            var titleGo = new GameObject("PauseTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            titleGo.transform.SetParent(_pauseOverlay.transform, worldPositionStays: false);
            var titleRt = (RectTransform)titleGo.transform;
            titleRt.anchorMin = new Vector2(0.5f, 0.5f);
            titleRt.anchorMax = new Vector2(0.5f, 0.5f);
            titleRt.pivot = new Vector2(0.5f, 0.5f);
            titleRt.anchoredPosition = new Vector2(0f, 170f);
            titleRt.sizeDelta = new Vector2(500f, 100f);

            var title = titleGo.GetComponent<Text>();
            title.text = "PAUSED";
            title.alignment = TextAnchor.MiddleCenter;
            title.fontSize = 56;
            title.fontStyle = FontStyle.Bold;
            title.color = Color.white;
            title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (title.font == null) title.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            CreatePressButton(_pauseOverlay.transform, "ResumeBtn", "RESUME",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, 30f), 360f, ResumeGame);

            CreatePressButton(_pauseOverlay.transform, "MenuBtn", "MAIN MENU",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0f, -120f), 360f, QuitToMenu);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }

            if (IsGamePaused())
            {
                _inputData.MoveForward = false;
                _inputData.MoveBackward = false;
                _inputData.Crouch = false;
                _inputData.JumpPressed = false;
                _inputData.PunchPressed = false;
                _inputData.KickPressed = false;
                if (_player != null) _player.InputData = _inputData;
                return;
            }

            // Reinicia flags continuos cada frame y los reconstruye desde teclado + táctil (OR lógico).
            _inputData.MoveForward  = _touchMoveForward;
            _inputData.MoveBackward = _touchMoveBackward;
            _inputData.Crouch       = _touchCrouch;

            // Mezcla teclado (suma OR)
            float h = Input.GetAxisRaw("Horizontal");
            if (h >  0.1f) _inputData.MoveForward  = true;
            if (h < -0.1f) _inputData.MoveBackward = true;

            // Crouch: tecla S/DownArrow (no usamos input axes para evitar errores si "Crouch" no está definido en InputManager).
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) _inputData.Crouch = true;

            // Jump: Space / W / UpArrow
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                _inputData.JumpPressed = true;

            // Punch: J  | Kick: K
            if (Input.GetKeyDown(KeyCode.J)) _inputData.PunchPressed = true;
            if (Input.GetKeyDown(KeyCode.K)) _inputData.KickPressed  = true;

            if (_player != null) _player.InputData = _inputData;
        }

        private void LateUpdate()
        {
            // Reset edge-triggered DESPUÉS de que PlayerController los consumiera en Update().
            _inputData.JumpPressed  = false;
            _inputData.PunchPressed = false;
            _inputData.KickPressed  = false;

            // FIX P2-1: NO reescribimos _player.InputData aquí. Como PlayerController ya consumió los
            // flags en su Update() (ScriptExecutionOrder -100 vs 0), volver a copiar el struct con los
            // flags reseteados es redundante. El próximo Update() de este handler reconstruye el snapshot.
        }

        // FIX G-07: garantizar que los flags edge se borren si la app pierde foco / GO se desactiva.
        private void OnDisable() => ResetAllFlags();
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                ResetAllFlags();
                PauseGame();
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                ResetAllFlags();
                PauseGame();
            }
        }

        private void ResetAllFlags()
        {
            _touchMoveForward = false;
            _touchMoveBackward = false;
            _touchCrouch = false;
            _inputData.MoveForward = false;
            _inputData.MoveBackward = false;
            _inputData.Crouch = false;
            _inputData.JumpPressed = false;
            _inputData.PunchPressed = false;
            _inputData.KickPressed = false;
            if (_player != null) _player.InputData = _inputData;
        }
    }
}
