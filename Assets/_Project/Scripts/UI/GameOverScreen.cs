using System.Collections;
using GemGrid.Journey;
using GemGrid.PowerUps;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Game Over / Level Result overlay: shown when <see cref="GameManager.GameOver"/>
    /// fires, hidden again on <see cref="GameManager.GameRestarted"/> or a successful
    /// <see cref="GameManager.TryContinueAfterGameOver"/>. In Classic Mode this behaves
    /// exactly as before M3/V2 (score, best score, Restart, Main Menu). During a Journey/
    /// Daily attempt (<see cref="GameplaySessionStarter.ActiveSession"/> non-null) it also
    /// shows the win/lose result and stars, records the result via
    /// <see cref="GemGrid.UI.MetaProgressionService"/>, and offers Continue (on a loss,
    /// while still available) and Next Level (on a win).
    ///
    /// Lives in the UI assembly (not Gameplay) so it can reference GemGrid.Journey/PowerUps
    /// without Gameplay depending back on them — see GameplaySessionStarter.cs.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see UNITY_SETUP.md.
    /// </summary>
    public class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup panelCanvasGroup;
        [SerializeField] private Text titleText;
        [SerializeField] private Text reasonOrResultText;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text rewardText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string gameplaySceneName = "Gameplay";

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
            if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
            if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        }

        private void Start()
        {
            var game = _gameManagerBehaviour.Game;
            game.GameOver += OnGameOver;
            game.GameRestarted += OnHidePanel;
            game.ContinuedAfterGameOver += OnHidePanel;
        }

        private void OnDestroy()
        {
            var game = _gameManagerBehaviour != null ? _gameManagerBehaviour.Game : null;
            if (game == null) return;
            game.GameOver -= OnGameOver;
            game.GameRestarted -= OnHidePanel;
            game.ContinuedAfterGameOver -= OnHidePanel;
        }

        private void OnGameOver()
        {
            int finalScore = _gameManagerBehaviour.Game.Score.TotalScore;
            int best = BestScoreStore.SubmitScore(finalScore);
            if (finalScoreText != null) finalScoreText.text = $"Score {finalScore}";
            if (bestScoreText != null) bestScoreText.text = $"Best {best}";

            var session = GameplaySessionStarter.ActiveSession;
            if (session != null) ShowJourneyResult(session);
            else ShowClassicResult();

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
                if (panelCanvasGroup != null) StartCoroutine(FadeInPanel());
            }
        }

        private void ShowClassicResult()
        {
            if (titleText != null) titleText.text = "GAME OVER";
            if (reasonOrResultText != null) reasonOrResultText.text = "No more valid moves.";
            if (rewardText != null) rewardText.gameObject.SetActive(false);
            if (continueButton != null) continueButton.gameObject.SetActive(false);
            if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(false);
        }

        private void ShowJourneyResult(JourneyLevelSession session)
        {
            int stars = session.CalculateStars();
            bool won = session.Result == LevelAttemptResult.Won;

            if (titleText != null) titleText.text = won ? "LEVEL COMPLETE" : "LEVEL FAILED";
            if (reasonOrResultText != null) reasonOrResultText.text = won ? StarsToText(stars) : "No more valid moves.";

            if (won)
            {
                var reward = GemGrid.UI.MetaProgressionService.Instance != null
                    ? GemGrid.UI.MetaProgressionService.Instance.RecordLevelResult(session.Level, stars)
                    : default;
                if (rewardText != null)
                {
                    rewardText.gameObject.SetActive(true);
                    rewardText.text = $"+{reward.Coins} Coins   +{reward.Xp} XP" + (reward.Gems > 0 ? $"   +{reward.Gems} Gems" : string.Empty);
                }
            }
            else if (rewardText != null)
            {
                rewardText.gameObject.SetActive(false);
            }

            bool offerContinue = !won && _gameManagerBehaviour.Game.CanContinue;
            if (continueButton != null) continueButton.gameObject.SetActive(offerContinue);
            if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(won);
        }

        private static string StarsToText(int stars)
        {
            switch (stars)
            {
                case 3: return "* * *";
                case 2: return "* * _";
                case 1: return "* _ _";
                default: return "_ _ _";
            }
        }

        private void OnHidePanel()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void OnRestartClicked()
        {
            var session = GameplaySessionStarter.ActiveSession;
            if (session != null)
            {
                // Journey/Daily: rebuild the level from scratch (fresh pre-fill/objective
                // state) rather than GameManagerBehaviour.Restart(), which would only
                // reset the grid/score/combo of the *same* GameManager instance and leave
                // stale objective progress behind.
                PendingGameplayIntent.RequestJourneyLevel(session.Level.GlobalIndex);
                SceneManager.LoadScene(gameplaySceneName);
            }
            else
            {
                _gameManagerBehaviour.Restart(); // Classic Mode — unchanged since M1.5/M2.
            }
        }

        private void OnMainMenuClicked() => SceneManager.LoadScene(mainMenuSceneName);

        private void OnContinueClicked() => _gameManagerBehaviour.Game.TryContinueAfterGameOver();

        private void OnNextLevelClicked()
        {
            var session = GameplaySessionStarter.ActiveSession;
            if (session == null) return;

            int nextIndex = session.Level.GlobalIndex + 1;
            var meta = GemGrid.UI.MetaProgressionService.Instance;
            bool hasEnergy = meta == null || meta.Energy.TrySpend(1); // no meta service yet (shouldn't happen in practice) -> don't block play

            if (!hasEnergy)
            {
                SceneManager.LoadScene(mainMenuSceneName); // no Energy left — send the player back rather than block them here.
                return;
            }

            PendingGameplayIntent.RequestJourneyLevel(nextIndex);
            SceneManager.LoadScene(gameplaySceneName);
        }

        /// <summary>Fade + scale-up intro so Game Over doesn't just snap on screen instantly.</summary>
        private IEnumerator FadeInPanel()
        {
            var rect = panelCanvasGroup.GetComponent<RectTransform>();
            const float duration = 0.22f;
            const float startScale = 0.85f;
            panelCanvasGroup.alpha = 0f;
            if (rect != null) rect.localScale = Vector3.one * startScale;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = t / duration;
                panelCanvasGroup.alpha = Mathf.Lerp(0f, 1f, k);
                if (rect != null) rect.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one, k);
                yield return null;
            }
            panelCanvasGroup.alpha = 1f;
            if (rect != null) rect.localScale = Vector3.one;
        }
    }
}
