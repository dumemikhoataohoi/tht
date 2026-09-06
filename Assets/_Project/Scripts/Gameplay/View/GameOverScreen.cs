using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Game Over overlay: shown when <see cref="GameManager.GameOver"/> fires, hidden
    /// again on <see cref="GameManager.GameRestarted"/>. Shows the final score and the
    /// best score (updated via <see cref="BestScoreStore"/>), with Restart and Main
    /// Menu buttons. Real UGUI (Canvas), placeholder colors/layout only.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see README_M1.md.
    /// </summary>
    public class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        private GameManagerBehaviour _gameManagerBehaviour;

        private void Awake()
        {
            _gameManagerBehaviour = GameManagerBehaviour.Instance;
            if (_gameManagerBehaviour == null)
                throw new System.InvalidOperationException(
                    $"{nameof(GameOverScreen)} requires the game to be bootstrapped from the Boot scene first " +
                    "(GameManagerBehaviour.Instance is null) — see UNITY_SETUP.md.");

            if (panelRoot != null) panelRoot.SetActive(false);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void Start()
        {
            var game = _gameManagerBehaviour.Game;
            game.GameOver += OnGameOver;
            game.GameRestarted += OnRestarted;
        }

        private void OnDestroy()
        {
            var game = _gameManagerBehaviour != null ? _gameManagerBehaviour.Game : null;
            if (game == null) return;
            game.GameOver -= OnGameOver;
            game.GameRestarted -= OnRestarted;
        }

        private void OnGameOver()
        {
            int finalScore = _gameManagerBehaviour.Game.Score.TotalScore;
            int best = BestScoreStore.SubmitScore(finalScore);

            if (finalScoreText != null) finalScoreText.text = $"Score {finalScore}";
            if (bestScoreText != null) bestScoreText.text = $"Best {best}";
            if (panelRoot != null) panelRoot.SetActive(true);
        }

        private void OnRestarted()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void OnRestartClicked() => _gameManagerBehaviour.Restart();

        private void OnMainMenuClicked() => SceneManager.LoadScene(mainMenuSceneName);
    }
}
