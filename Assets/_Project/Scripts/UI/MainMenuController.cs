using GemGrid.Gameplay;
using GemGrid.Journey;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GemGrid.UI
{
    /// <summary>
    /// Main Menu screen: title, best score, and entry points for all three ways to play —
    /// Classic (unchanged since M1.5/M2), Journey (level select — see JourneyMap.unity),
    /// and Daily Challenge (jumps straight into a seeded Gameplay attempt). Placeholder
    /// colors/layout only — original branding, no copied UI/art per LICENSE_MANIFEST.md.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see UNITY_SETUP.md.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button journeyButton;
        [SerializeField] private Button dailyChallengeButton;
        [SerializeField] private string gameplaySceneName = "Gameplay";
        [SerializeField] private string journeyMapSceneName = "JourneyMap";

        private void Awake()
        {
            if (startGameButton != null) startGameButton.onClick.AddListener(OnStartGameClicked);
            if (journeyButton != null) journeyButton.onClick.AddListener(OnJourneyClicked);
            if (dailyChallengeButton != null) dailyChallengeButton.onClick.AddListener(OnDailyChallengeClicked);
        }

        private void Start()
        {
            if (bestScoreText != null)
                bestScoreText.text = $"Best {BestScoreStore.Get()}";
        }

        private void OnStartGameClicked()
        {
            PendingGameplayIntent.RequestClassic();
            SceneManager.LoadScene(gameplaySceneName);
        }

        private void OnJourneyClicked() => SceneManager.LoadScene(journeyMapSceneName);

        private void OnDailyChallengeClicked()
        {
            PendingGameplayIntent.RequestDailyChallenge();
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}
