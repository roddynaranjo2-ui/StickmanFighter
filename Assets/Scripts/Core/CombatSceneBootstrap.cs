// CombatSceneBootstrap.cs — Bootstrap defensivo que se autoinstala al cargar la CombatScene.
//
// Su trabajo es resolver:
//   - C-07: InfiniteGround no instanciado en la escena.
//   - G-02: instanciar un Enemy real en la escena para tener gameplay completo.
//   - G-02: añadir hitboxes Punch/Kick al Player como hijos.
//   - G-06: garantizar AudioManager presente.
//   - G-07: ScreenShake en cámara.
//   - G-09: GameOverUI + HealthBars en HUD.
//
// SPRINT #3 (v0.1.4): expandido masivamente — antes solo creaba InfiniteGround.

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using StickmanFighter.Environment;
using StickmanFighter.Character;
using StickmanFighter.Combat;
using StickmanFighter.Enemy;
using StickmanFighter.Audio;
using StickmanFighter.VFX;
using StickmanFighter.UI;

namespace StickmanFighter.Core
{
    public static class CombatSceneBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterHook()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;   // idempotente
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "CombatScene") return;

            EnsureAudioManager();
            EnsureInfiniteGround();
            EnsureScreenShake();
            EnsurePlayerHitboxes();
            EnsureEnemy();
            EnsureHud();
        }

        private static void EnsureInfiniteGround()
        {
            var existing = Object.FindAnyObjectByType<InfiniteGround>();
            if (existing != null) return;
            var go = new GameObject("InfiniteGround");
            go.AddComponent<InfiniteGround>();
        }

        private static void EnsureAudioManager()
        {
            if (AudioManager.Instance != null) return;
            var go = new GameObject("AudioManager");
            go.AddComponent<AudioManager>();
        }

        private static void EnsureScreenShake()
        {
            var cam = Camera.main;
            if (cam == null) return;
            if (cam.gameObject.GetComponent<ScreenShake>() == null)
                cam.gameObject.AddComponent<ScreenShake>();
        }

        private static void EnsurePlayerHitboxes()
        {
            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player == null) return;

            // HitFlash en Player
            if (player.GetComponent<HitFlashFx>() == null)
                player.gameObject.AddComponent<HitFlashFx>();

            // Hitboxes hijas (Punch / Kick)
            EnsureHitboxChild(player.transform, "PunchHitbox", AttackType.Punch, damage: 10,
                              offset: new Vector2(0.55f, 0.45f), size: new Vector2(0.7f, 0.4f));
            EnsureHitboxChild(player.transform, "KickHitbox",  AttackType.Kick,  damage: 15,
                              offset: new Vector2(0.65f, 0.15f), size: new Vector2(0.9f, 0.5f));
        }

        private static void EnsureHitboxChild(Transform parent, string name, AttackType type, int damage,
                                              Vector2 offset, Vector2 size)
        {
            var existing = parent.Find(name);
            GameObject go;
            if (existing == null)
            {
                go = new GameObject(name);
                go.transform.SetParent(parent, worldPositionStays: false);
            }
            else
            {
                go = existing.gameObject;
            }

            var hb = go.GetComponent<Hitbox>();
            if (hb == null) hb = go.AddComponent<Hitbox>();
            hb.Damage = damage;
            hb.AttackType = type;
            hb.Offset  = offset;
            hb.SizeBox = size;
            hb.Deactivate();
        }

        private static void EnsureEnemy()
        {
            if (Object.FindFirstObjectByType<EnemyController>() != null) return;

            var player = Object.FindFirstObjectByType<PlayerController>();
            float px = player != null ? player.transform.position.x : 0f;

            // Crear GO Enemy con sprite simple
            var enemy = new GameObject("Enemy");
            enemy.transform.position = new Vector3(px + 5f, 1f, 0f);

            var sr = enemy.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.85f, 0.2f, 0.2f, 1f);
            sr.sprite = CreateRectSprite();
            sr.sortingOrder = 5;

            var rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var col = enemy.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.6f, 1.8f);

            enemy.AddComponent<HealthSystem>();
            enemy.AddComponent<EnemyController>();
            enemy.AddComponent<HitFlashFx>();

            // Hitbox del enemigo (ataque cuerpo a cuerpo)
            var attackGo = new GameObject("EnemyHitbox");
            attackGo.transform.SetParent(enemy.transform, false);
            var ehb = attackGo.AddComponent<Hitbox>();
            ehb.Damage = 8;
            ehb.Deactivate();

            // Layer + tag
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            if (enemyLayer >= 0) enemy.layer = enemyLayer;
        }

        private static void EnsureHud()
        {
            // Buscar Canvas existente o crear uno
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                var go = new GameObject("HudCanvas");
                canvas = go.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                go.AddComponent<CanvasScaler>();
                go.AddComponent<GraphicRaycaster>();
            }

            EnsurePlayerHealthBar(canvas);
            EnsureEnemyHealthBar(canvas);
            EnsureGameOverPanel(canvas);
        }

        private static void EnsurePlayerHealthBar(Canvas canvas)
        {
            if (canvas.transform.Find("PlayerHealthBar") != null) return;
            var player = Object.FindFirstObjectByType<PlayerController>();
            if (player == null) return;

            var bar = CreateBar("PlayerHealthBar", canvas.transform,
                                anchorMin: new Vector2(0f, 1f), anchorMax: new Vector2(0f, 1f),
                                pivot:     new Vector2(0f, 1f), pos: new Vector2(20f, -20f),
                                size: new Vector2(300f, 26f));
            var hb = bar.go.AddComponent<HealthBarUI>();
            var sfField = hb.GetType().GetField("_fill",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (sfField != null) sfField.SetValue(hb, bar.fill);
            hb.Bind(player.Health);
        }

        private static void EnsureEnemyHealthBar(Canvas canvas)
        {
            if (canvas.transform.Find("EnemyHealthBar") != null) return;
            var enemy = Object.FindFirstObjectByType<EnemyController>();
            if (enemy == null) return;

            var bar = CreateBar("EnemyHealthBar", canvas.transform,
                                anchorMin: new Vector2(1f, 1f), anchorMax: new Vector2(1f, 1f),
                                pivot:     new Vector2(1f, 1f), pos: new Vector2(-20f, -20f),
                                size: new Vector2(300f, 26f));
            var hb = bar.go.AddComponent<HealthBarUI>();
            var sfField = hb.GetType().GetField("_fill",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (sfField != null) sfField.SetValue(hb, bar.fill);
            hb.Bind(enemy.Health);
        }

        private static void EnsureGameOverPanel(Canvas canvas)
        {
            if (canvas.transform.Find("GameOverPanel") != null) return;

            var panelGo = new GameObject("GameOverPanel");
            panelGo.transform.SetParent(canvas.transform, false);
            var panelRt = panelGo.AddComponent<RectTransform>();
            panelRt.anchorMin = Vector2.zero; panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero; panelRt.offsetMax = Vector2.zero;
            var bg = panelGo.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.75f);

            // Título
            var titleGo = new GameObject("Title");
            titleGo.transform.SetParent(panelGo.transform, false);
            var titleRt = titleGo.AddComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.5f); titleRt.anchorMax = new Vector2(0.5f, 0.5f);
            titleRt.pivot = new Vector2(0.5f, 0.5f);
            titleRt.anchoredPosition = new Vector2(0f, 100f);
            titleRt.sizeDelta = new Vector2(800f, 100f);
            var titleTxt = titleGo.AddComponent<Text>();
            titleTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleTxt.fontSize = 64;
            titleTxt.alignment = TextAnchor.MiddleCenter;
            titleTxt.color = Color.white;
            titleTxt.text = "GAME OVER";

            // Botón Retry
            var retryBtn = CreateUiButton("RetryButton", panelGo.transform, "REINTENTAR",
                                          new Vector2(0.5f, 0.5f), new Vector2(0f, -20f),
                                          new Vector2(280f, 60f));
            // Botón Menu
            var menuBtn  = CreateUiButton("MenuButton",  panelGo.transform, "MENÚ PRINCIPAL",
                                          new Vector2(0.5f, 0.5f), new Vector2(0f, -100f),
                                          new Vector2(280f, 60f));

            var go = panelGo.AddComponent<GameOverUI>();
            var t = go.GetType();
            var f = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            t.GetField("_panel", f)?.SetValue(go, panelGo);
            t.GetField("_titleLabel", f)?.SetValue(go, titleTxt);
            t.GetField("_retryButton", f)?.SetValue(go, retryBtn);
            t.GetField("_menuButton",  f)?.SetValue(go, menuBtn);

            panelGo.SetActive(false);
        }

        // ───────────────────── UI Helpers ─────────────────────
        private struct BarRefs { public GameObject go; public Image fill; }

        private static BarRefs CreateBar(string name, Transform parent,
                                         Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
                                         Vector2 pos, Vector2 size)
        {
            var bg = new GameObject(name);
            bg.transform.SetParent(parent, false);
            var bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchorMin = anchorMin; bgRt.anchorMax = anchorMax; bgRt.pivot = pivot;
            bgRt.anchoredPosition = pos; bgRt.sizeDelta = size;
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.6f);

            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(bg.transform, false);
            var fRt = fillGo.AddComponent<RectTransform>();
            fRt.anchorMin = new Vector2(0f, 0f); fRt.anchorMax = new Vector2(1f, 1f);
            fRt.offsetMin = new Vector2(3f, 3f); fRt.offsetMax = new Vector2(-3f, -3f);
            var fImg = fillGo.AddComponent<Image>();
            fImg.color = new Color(0.2f, 0.85f, 0.25f);
            fImg.type = Image.Type.Filled;
            fImg.fillMethod = Image.FillMethod.Horizontal;
            fImg.fillAmount = 1f;

            return new BarRefs { go = bg, fill = fImg };
        }

        private static Button CreateUiButton(string name, Transform parent, string label,
                                             Vector2 anchor, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos; rt.sizeDelta = size;
            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.45f, 0.85f, 1f);
            var btn = go.AddComponent<Button>();

            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(go.transform, false);
            var tRt = txtGo.AddComponent<RectTransform>();
            tRt.anchorMin = Vector2.zero; tRt.anchorMax = Vector2.one;
            tRt.offsetMin = Vector2.zero; tRt.offsetMax = Vector2.zero;
            var txt = txtGo.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 28;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = label;

            return btn;
        }

        private static Sprite CreateRectSprite()
        {
            // 32x96 sprite blanco para Enemy. Pivot inferior-centro.
            var tex = new Texture2D(32, 96, TextureFormat.RGBA32, false);
            var pixels = new Color[32 * 96];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 32, 96), new Vector2(0.5f, 0f), 64f);
        }
    }
}
