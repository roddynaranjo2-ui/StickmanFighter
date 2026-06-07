// GameManager.cs — Singleton persistente. Controla estado global y framerate móvil.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace StickmanFighter.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager? Instance { get; private set; }

        public enum GameState { MainMenu, Playing, Paused }
        public GameState CurrentState { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Mobile-specific: 60fps fijo.
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                Instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if      (scene.name == "CombatScene") SetState(GameState.Playing);
            else if (scene.name == "MainMenu")    SetState(GameState.MainMenu);
        }

        public void SetState(GameState newState)
        {
            CurrentState = newState;
            Time.timeScale = newState == GameState.Paused ? 0f : 1f;
        }

        public void StartGame()  => SceneLoader.Instance.LoadScene("CombatScene");
        public void PauseGame()  => SetState(GameState.Paused);
        public void ResumeGame() => SetState(GameState.Playing);

        private void OnApplicationPause(bool paused)
        {
            if (paused && CurrentState == GameState.Playing) PauseGame();
        }
    }
}
