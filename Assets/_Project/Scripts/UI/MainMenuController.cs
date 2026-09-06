using GemGrid.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GemGrid.UI
{
    /// <summary>
    /// Main Menu screen: title, best score, and a Start Game button that loads the
    /// Gameplay scene. Placeholder colors/layout only — original branding, no
    /// copied UI/art per LICENSE_MANIFEST.md.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see README_M1.md.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Button startGameButton;
        [SerializeField] private string gameplaySceneName = "Gameplay";

        private void Awake()
        {
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);
        }

        private void Start()
        {
            if (bestScoreText != null)
                bestScoreText.text = $"Best {BestScoreStore.Get()}";
        }

        private void OnStartGameClicked() => SceneManager.LoadScene(gameplaySceneName);
    }
}
