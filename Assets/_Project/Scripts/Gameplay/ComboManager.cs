using System;
using GemGrid.Configuration;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Tracks the consecutive-clear combo streak. A move that clears at least one line
    /// increments the combo; a move that clears nothing resets it to zero.
    /// </summary>
    public class ComboManager
    {
        private readonly ComboRules _rules;

        public int CurrentCombo { get; private set; }
        public int BestCombo { get; private set; }

        public event Action<int> ComboChanged;

        public ComboManager(ComboRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public float CurrentMultiplier => _rules.GetMultiplierForCombo(CurrentCombo);

        public void RegisterMove(bool didClearLines)
        {
            if (didClearLines)
            {
                CurrentCombo++;
                if (CurrentCombo > BestCombo) BestCombo = CurrentCombo;
            }
            else
            {
                CurrentCombo = 0;
            }
            ComboChanged?.Invoke(CurrentCombo);
        }

        public void Reset()
        {
            CurrentCombo = 0;
            BestCombo = 0;
            ComboChanged?.Invoke(CurrentCombo);
        }

        /// <summary>Restores values captured before a move, for the single-step Undo power-up. Internal — only <see cref="GameManager"/> (same assembly) needs it.</summary>
        internal void RestoreState(int currentCombo, int bestCombo)
        {
            CurrentCombo = currentCombo;
            BestCombo = bestCombo;
            ComboChanged?.Invoke(CurrentCombo);
        }
    }
}
