// GameOverUI.cs — Overlay de fin de partida. Se muestra cuando Player o Enemy muere.
// FIX G-09 SPRINT #3.

using UnityEngine;
using UnityEngine.UI;
using StickmanFighter.Character;
using StickmanFighter.Core;
using StickmanFighter.Enemy;

namespace StickmanFighter.UI
{
    public sealed class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject? _panel;
        [SerializeField] private Text? _titleLabel;
        [SerializeField] private Button? _retryButton;
        [SerializeField] private Button? _menuButton;

        private PlayerController? _player;
        private EnemyController? _enemy;
        private bool _shown;

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerController>();
            _enemy  = FindFirstObjectByType<EnemyController>();

            if (_player != null) _player.Health.OnDied += OnPlayerDied;
            if (_enemy  != null) _enemy.Health.OnDied  += OnEnemyDied;

            if (_panel != null) _panel.SetActive(false);
            if (_retryButton != null) _retryButton.onClick.AddListener(OnRetry);
            if (_menuButton  != null) _menuButton.onClick.AddListener(OnMenu);
        }

        private void OnDestroy()
        {
            if (_player != null) _player.Health.OnDied -= OnPlayerDied;
            if (_enemy  != null) _enemy.Health.OnDied  -= OnEnemyDied;
        }

        private void OnPlayerDied() => Show("DERROTA");
        private void OnEnemyDied()  => Show("¡VICTORIA!");

        private void Show(string title)
        {
            if (_shown) return;
            _shown = true;
            if (_panel != null) _panel.SetActive(true);
            if (_titleLabel != null) _titleLabel.text = title;
            Time.timeScale = 0.15f; // slowmotion dramático
        }

        private void OnRetry()
        {
            Time.timeScale = 1f;
            var loader = FindFirstObjectByType<SceneLoader>();
            if (loader != null) loader.LoadCombat();
            else UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        private void OnMenu()
        {
            Time.timeScale = 1f;
            var loader = FindFirstObjectByType<SceneLoader>();
            if (loader != null) loader.LoadMainMenu();
            else UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
}
