using UnityEngine;
using UnityEngine.SceneManagement;

namespace GemGrid.Bootstrap
{
    /// <summary>
    /// Lives in the Boot scene next to <see cref="GemGrid.Gameplay.GameManagerBehaviour"/>.
    /// Its only job is to hand off to the Gameplay scene once the Boot scene's own
    /// Awake/Start pass has finished initializing services, GameManager, and
    /// configuration — see UNITY_SETUP.md for the full Boot ▸ Gameplay flow.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see README_M1.md.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private string gameplaySceneName = "Gameplay";

        private void Start()
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}
