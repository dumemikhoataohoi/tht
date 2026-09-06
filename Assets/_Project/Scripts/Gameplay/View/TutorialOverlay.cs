using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// First-time-only "how to play" overlay: shown once (tracked via a
    /// <see cref="PlayerPrefs"/> flag, same lightweight persistence approach as
    /// <see cref="BestScoreStore"/>) the very first time the Gameplay scene starts,
    /// dismissible via a single button, never shown again afterwards. Pure UI — doesn't
    /// touch GameManager/scoring/grid logic.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see UNITY_SETUP.md.
    /// </summary>
    public class TutorialOverlay : MonoBehaviour
    {
        private const string SeenPrefKey = "GemGrid_TutorialSeen";

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button dismissButton;

        private void Awake()
        {
            if (dismissButton != null) dismissButton.onClick.AddListener(Dismiss);
        }

        private void Start()
        {
            bool alreadySeen = PlayerPrefs.GetInt(SeenPrefKey, 0) != 0;
            if (panelRoot != null) panelRoot.SetActive(!alreadySeen);
        }

        private void Dismiss()
        {
            PlayerPrefs.SetInt(SeenPrefKey, 1);
            PlayerPrefs.Save();

            // Deactivating panelRoot here would also deactivate the dismiss button's own
            // GameObject (it's a child of panelRoot) mid-click — this listener runs as
            // part of the same Button.onClick dispatch as the button's other listeners
            // (ButtonPunchFeedback/ButtonClickSfx), and one of those calls StartCoroutine
            // on that same GameObject. If it runs after the GameObject is already
            // deactivated, Unity throws "Coroutine couldn't be started because the game
            // object is inactive". Deferring the deactivation by one frame lets every
            // listener for this click finish first, regardless of registration order.
            StartCoroutine(DeactivatePanelNextFrame());
        }

        private IEnumerator DeactivatePanelNextFrame()
        {
            yield return null;
            if (panelRoot != null) panelRoot.SetActive(false);
        }
    }
}
