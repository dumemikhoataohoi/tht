using System.Collections;
using GemGrid.Journey;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GemGrid.UI
{
    /// <summary>
    /// One level tile on the Journey map: shows locked/unlocked + stars, spends 1 Gem
    /// Energy and starts the level on tap (see GAME_DESIGN_V2.md §5/§8). Energy is
    /// checked/spent here — before the Gameplay scene even loads — rather than inside
    /// GameplaySessionStarter, since "not enough Energy" must never let the player into
    /// a level in the first place.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see UNITY_SETUP.md.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class JourneyLevelButtonView : MonoBehaviour
    {
        [SerializeField] private int globalIndex;
        [SerializeField] private Text statusText;
        [SerializeField] private string gameplaySceneName = "Gameplay";

        private const float NotEnoughEnergyFlashSeconds = 1.2f;

        /// <summary>Set once by GemGridSceneSetup right after creation — plain field assignment, no serialization round-trip needed since this runs at scene-build time.</summary>
        public void Initialize(int levelGlobalIndex, Text starOrStatusLabel)
        {
            globalIndex = levelGlobalIndex;
            statusText = starOrStatusLabel;
        }

        private void Awake() => GetComponent<Button>().onClick.AddListener(OnClicked);

        private void Start() => Refresh();

        public void Refresh()
        {
            var journey = MetaProgressionService.Instance != null ? MetaProgressionService.Instance.Journey : null;
            bool unlocked = journey == null || journey.IsUnlocked(globalIndex);

            GetComponent<Button>().interactable = unlocked;
            if (statusText != null)
                statusText.text = unlocked ? StarsToText(journey?.StarsFor(globalIndex) ?? 0) : "LOCKED";
        }

        private void OnClicked()
        {
            var meta = MetaProgressionService.Instance;
            if (meta != null && !meta.Energy.HasEnough(1))
            {
                if (statusText != null) StartCoroutine(FlashNotEnoughEnergy());
                return;
            }

            meta?.Energy.TrySpend(1);
            PendingGameplayIntent.RequestJourneyLevel(globalIndex);
            SceneManager.LoadScene(gameplaySceneName);
        }

        private IEnumerator FlashNotEnoughEnergy()
        {
            statusText.text = "NEED ENERGY";
            float t = 0f;
            while (t < NotEnoughEnergyFlashSeconds)
            {
                t += Time.deltaTime;
                yield return null;
            }
            Refresh();
        }

        private static string StarsToText(int stars)
        {
            switch (stars)
            {
                case 3: return "* * *";
                case 2: return "* * _";
                case 1: return "* _ _";
                default: return "PLAY";
            }
        }
    }
}
