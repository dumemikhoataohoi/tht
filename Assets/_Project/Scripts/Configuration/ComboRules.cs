using System;

namespace GemGrid.Configuration
{
    /// <summary>Tunable combo multiplier curve. Authored via <see cref="GameplayConfig"/>.</summary>
    [Serializable]
    public class ComboRules
    {
        public float MultiplierPerComboStep = 0.1f;
        public float MaxMultiplier = 3.0f;

        public float GetMultiplierForCombo(int comboCount)
        {
            if (comboCount < 0) throw new ArgumentOutOfRangeException(nameof(comboCount));
            float multiplier = 1f + comboCount * MultiplierPerComboStep;
            return Math.Min(multiplier, MaxMultiplier);
        }
    }
}
