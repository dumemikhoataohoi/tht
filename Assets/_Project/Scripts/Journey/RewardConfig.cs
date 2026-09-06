namespace GemGrid.Journey
{
    /// <summary>
    /// The "RewardConfig" data referenced in the implementation brief — every reward
    /// number used by <see cref="LevelRewardCalculator"/>/<see cref="ChapterRewardCalculator"/>,
    /// kept out of that logic so balance can be retuned without touching code.
    /// </summary>
    public class RewardConfig
    {
        public int CoinsPerStar = 20;
        public int FirstClearBonusCoins = 20;
        public int XpPerLevelWin = 10;
        public int FirstClearBonusXp = 10;
        public int ThreeStarFirstTimeGemBonus = 1;

        public int ChapterChestCoins = 150;
        public int ChapterChestGems = 3;
        public int ChapterAllThreeStarChestCoins = 300;
        public int ChapterAllThreeStarChestGems = 8;
    }
}
