using UnityEngine;

namespace GemGrid.Configuration
{
    /// <summary>
    /// Bundles scoring and combo tuning into one data asset. Create via
    /// Assets ▸ Create ▸ GemGrid ▸ Configuration ▸ Gameplay Config.
    /// </summary>
    [CreateAssetMenu(menuName = "GemGrid/Configuration/Gameplay Config", fileName = "GameplayConfig")]
    public class GameplayConfig : ScriptableObject, IGameplayConfigProvider
    {
        [SerializeField] private ScoreRules scoreRules = new ScoreRules();
        [SerializeField] private ComboRules comboRules = new ComboRules();

        public ScoreRules ScoreRules => scoreRules;
        public ComboRules ComboRules => comboRules;
    }
}
