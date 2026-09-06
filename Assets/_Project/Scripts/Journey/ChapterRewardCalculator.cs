namespace GemGrid.Journey
{
    public readonly struct ChapterChestReward
    {
        public readonly int Coins;
        public readonly int Gems;

        public ChapterChestReward(int coins, int gems)
        {
            Coins = coins;
            Gems = gems;
        }
    }

    /// <summary>Chapter-completion Chest reward — see GAME_DESIGN_V2.md §11.</summary>
    public static class ChapterRewardCalculator
    {
        public static ChapterChestReward CalculateChestReward(RewardConfig config, bool allThreeStars) =>
            allThreeStars
                ? new ChapterChestReward(config.ChapterAllThreeStarChestCoins, config.ChapterAllThreeStarChestGems)
                : new ChapterChestReward(config.ChapterChestCoins, config.ChapterChestGems);
    }
}
