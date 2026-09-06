using System;
using GemGrid.Configuration;

namespace GemGrid.Gameplay
{
    /// <summary>Tracks total score for the current session using data-driven <see cref="ScoreRules"/>.</summary>
    public class ScoreManager
    {
        private readonly ScoreRules _rules;

        public int TotalScore { get; private set; }

        public event Action<int> ScoreChanged;

        public ScoreManager(ScoreRules rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        public void RegisterPlacement(int cellsPlaced) => AddScore(_rules.CalculatePlacementScore(cellsPlaced));

        public void RegisterClear(int linesCleared, float comboMultiplier)
        {
            int baseScore = _rules.CalculateClearScore(linesCleared);
            int finalScore = (int)Math.Round(baseScore * comboMultiplier, MidpointRounding.AwayFromZero);
            AddScore(finalScore);
        }

        public void Reset()
        {
            TotalScore = 0;
            ScoreChanged?.Invoke(TotalScore);
        }

        /// <summary>Restores a value captured before a move, for the single-step Undo power-up. Internal — only <see cref="GameManager"/> (same assembly) needs it.</summary>
        internal void RestoreTotalScore(int value)
        {
            if (TotalScore == value) return;
            TotalScore = value;
            ScoreChanged?.Invoke(TotalScore);
        }

        private void AddScore(int amount)
        {
            if (amount == 0) return;
            TotalScore += amount;
            ScoreChanged?.Invoke(TotalScore);
        }
    }
}
