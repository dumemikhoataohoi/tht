namespace GemGrid.Journey
{
    /// <summary>
    /// Star rating for a finished Journey attempt — see GAME_DESIGN_V2.md §5. Concrete
    /// interpretation chosen for the 2nd star ("còn dư sức chơi tiếp"): completing the
    /// primary objective within <see cref="EfficientMovesThreshold"/> placements, a
    /// cleanly-computable proxy for "efficient/comfortable win" that rewards skilled play
    /// without needing extra board-state heuristics. Tune via playtest, not a hard rule.
    /// </summary>
    public static class StarCalculator
    {
        public const int EfficientMovesThreshold = 24;

        /// <param name="primaryComplete">Whether the level's primary objective was reached at all — 0 stars (a loss) if not.</param>
        /// <param name="movesUsedWhenPrimaryCompleted">From <see cref="ObjectiveTracker.MovesUsedWhenPrimaryCompleted"/>; -1 if never completed.</param>
        /// <param name="bonusComplete">Whether the optional bonus objective was also reached — always worth the 3rd star regardless of move count.</param>
        public static int CalculateStars(bool primaryComplete, int movesUsedWhenPrimaryCompleted, bool bonusComplete)
        {
            if (!primaryComplete) return 0;
            if (bonusComplete) return 3;
            if (movesUsedWhenPrimaryCompleted >= 0 && movesUsedWhenPrimaryCompleted <= EfficientMovesThreshold) return 2;
            return 1;
        }
    }
}
