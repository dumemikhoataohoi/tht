using System;

namespace GemGrid.Journey
{
    /// <summary>
    /// Player Rank derived purely from cumulative XP — see GAME_DESIGN_V2.md §14.
    /// Quadratic curve (rank R needs R² × 50 total XP) so early ranks come quickly and
    /// later ranks take progressively longer, without ever gating any content behind rank
    /// (rank only unlocks cosmetics/energy top-ups per the design doc).
    /// </summary>
    public static class PlayerRank
    {
        private const int XpPerRankSquared = 50;

        public static int XpRequiredForRank(int rank) => rank <= 0 ? 0 : rank * rank * XpPerRankSquared;

        public static int RankForXp(int totalXp)
        {
            if (totalXp < 0) totalXp = 0;
            int rank = 0;
            while (XpRequiredForRank(rank + 1) <= totalXp) rank++;
            return rank;
        }

        public static int XpIntoCurrentRank(int totalXp) => Math.Max(0, totalXp) - XpRequiredForRank(RankForXp(totalXp));

        public static int XpNeededForNextRank(int totalXp)
        {
            int rank = RankForXp(totalXp);
            return XpRequiredForRank(rank + 1) - XpRequiredForRank(rank);
        }
    }
}
