using System;

namespace GemGrid.Configuration
{
    /// <summary>Tunable scoring formula. Authored via <see cref="GameplayConfig"/>, never hard-coded.</summary>
    [Serializable]
    public class ScoreRules
    {
        public int ScorePerPlacedCell = 1;
        public int BaseScorePerLine = 10;

        /// <summary>Extra multiplier added per additional line cleared in the same move.</summary>
        public float MultiLineBonusFactor = 0.5f;

        public int CalculatePlacementScore(int cellsPlaced)
        {
            if (cellsPlaced < 0) throw new ArgumentOutOfRangeException(nameof(cellsPlaced));
            return cellsPlaced * ScorePerPlacedCell;
        }

        public int CalculateClearScore(int linesCleared)
        {
            if (linesCleared <= 0) return 0;

            double multiplier = 1d + (linesCleared - 1) * MultiLineBonusFactor;
            double raw = BaseScorePerLine * linesCleared * multiplier;
            return (int)Math.Round(raw, MidpointRounding.AwayFromZero);
        }
    }
}
