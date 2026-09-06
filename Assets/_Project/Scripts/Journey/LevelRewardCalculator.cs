namespace GemGrid.Journey
{
    public readonly struct LevelRewardResult
    {
        public readonly int Stars;
        public readonly int Coins;
        public readonly int Xp;
        public readonly int Gems;

        public LevelRewardResult(int stars, int coins, int xp, int gems)
        {
            Stars = stars;
            Coins = coins;
            Xp = xp;
            Gems = gems;
        }
    }

    /// <summary>Per-level reward on completion — see GAME_DESIGN_V2.md §11.</summary>
    public static class LevelRewardCalculator
    {
        public static LevelRewardResult Calculate(RewardConfig config, int stars, bool isFirstTimeClear, bool isFirstTimeThreeStar)
        {
            if (stars <= 0) return new LevelRewardResult(0, 0, 0, 0);

            int coins = stars * config.CoinsPerStar + (isFirstTimeClear ? config.FirstClearBonusCoins : 0);
            int xp = config.XpPerLevelWin + (isFirstTimeClear ? config.FirstClearBonusXp : 0);
            int gems = (stars == 3 && isFirstTimeThreeStar) ? config.ThreeStarFirstTimeGemBonus : 0;

            return new LevelRewardResult(stars, coins, xp, gems);
        }
    }
}
