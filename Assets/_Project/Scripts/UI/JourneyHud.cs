using GemGrid.Gameplay;
using GemGrid.Journey;
using UnityEngine;
using UnityEngine.UI;

namespace GemGrid.UI
{
    /// <summary>
    /// Journey-only HUD strip: current objective + progress, and the Gem Energy balance.
    /// Hides itself entirely in Classic Mode (<see cref="GameplaySessionStarter.ActiveSession"/>
    /// is null there) so it never interferes with the existing Classic HUD/board.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see UNITY_SETUP.md.
    /// </summary>
    public class JourneyHud : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text energyText;

        private System.Action<int> _onEnergyChanged;

        private void Start()
        {
            var session = GameplaySessionStarter.ActiveSession;
            if (session == null)
            {
                if (panelRoot != null) panelRoot.SetActive(false);
                return;
            }

            if (panelRoot != null) panelRoot.SetActive(true);
            session.Objectives.ObjectiveProgressChanged += RefreshObjectiveText;
            RefreshObjectiveText();

            if (MetaProgressionService.Instance != null)
            {
                _onEnergyChanged = _ => RefreshEnergyText();
                MetaProgressionService.Instance.Energy.EnergyChanged += _onEnergyChanged;
            }
            RefreshEnergyText();
        }

        private void OnDestroy()
        {
            var session = GameplaySessionStarter.ActiveSession;
            if (session != null) session.Objectives.ObjectiveProgressChanged -= RefreshObjectiveText;

            if (MetaProgressionService.Instance != null && _onEnergyChanged != null)
                MetaProgressionService.Instance.Energy.EnergyChanged -= _onEnergyChanged;
        }

        private void RefreshObjectiveText()
        {
            if (objectiveText == null) return;
            var session = GameplaySessionStarter.ActiveSession;
            if (session == null) return;

            var progress = session.Objectives.Primary;
            objectiveText.text = $"{DescribeObjective(progress.Type)}: {progress.Current}/{progress.Target}";
        }

        private void RefreshEnergyText()
        {
            if (energyText == null || MetaProgressionService.Instance == null) return;
            energyText.text = $"Energy {MetaProgressionService.Instance.Energy.Current}";
        }

        private static string DescribeObjective(ObjectiveType type)
        {
            switch (type)
            {
                case ObjectiveType.Score: return "Reach score";
                case ObjectiveType.LinesCleared: return "Clear lines";
                case ObjectiveType.Combo: return "Reach combo";
                case ObjectiveType.MultiClear: return "Multi-clears";
                case ObjectiveType.FillSlot: return "Refill tray";
                case ObjectiveType.Survive: return "Place blocks";
                default: return "Objective";
            }
        }
    }
}
