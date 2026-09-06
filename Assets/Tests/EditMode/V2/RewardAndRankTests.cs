using GemGrid.Journey;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode.V2
{
    public class LevelRewardCalculatorTests
    {
        [Test]
        public void ZeroStars_YieldsNoReward()
        {
            var result = LevelRewardCalculator.Calculate(new RewardConfig(), stars: 0, isFirstTimeClear: true, isFirstTimeThreeStar: true);

            Assert.AreEqual(0, result.Coins);
            Assert.AreEqual(0, result.Xp);
            Assert.AreEqual(0, result.Gems);
        }

        [Test]
        public void FirstTimeClear_AddsBonusCoinsAndXp()
        {
            var config = new RewardConfig();

            var first = LevelRewardCalculator.Calculate(config, stars: 1, isFirstTimeClear: true, isFirstTimeThreeStar: false);
            var replay = LevelRewardCalculator.Calculate(config, stars: 1, isFirstTimeClear: false, isFirstTimeThreeStar: false);

            Assert.Greater(first.Coins, replay.Coins);
            Assert.Greater(first.Xp, replay.Xp);
        }

        [Test]
        public void ThreeStars_FirstTime_AwardsGemBonus_ButNotOnReplay()
        {
            var config = new RewardConfig();

            var firstThreeStar = LevelRewardCalculator.Calculate(config, stars: 3, isFirstTimeClear: false, isFirstTimeThreeStar: true);
            var replayThreeStar = LevelRewardCalculator.Calculate(config, stars: 3, isFirstTimeClear: false, isFirstTimeThreeStar: false);

            Assert.AreEqual(config.ThreeStarFirstTimeGemBonus, firstThreeStar.Gems);
            Assert.AreEqual(0, replayThreeStar.Gems);
        }

        [Test]
        public void MoreStars_NeverYieldsFewerCoins()
        {
            var config = new RewardConfig();

            var oneStar = LevelRewardCalculator.Calculate(config, 1, false, false);
            var twoStar = LevelRewardCalculator.Calculate(config, 2, false, false);
            var threeStar = LevelRewardCalculator.Calculate(config, 3, false, false);

            Assert.LessOrEqual(oneStar.Coins, twoStar.Coins);
            Assert.LessOrEqual(twoStar.Coins, threeStar.Coins);
        }
    }

    public class ChapterRewardCalculatorTests
    {
        [Test]
        public void AllThreeStarChest_IsBetterThanNormalChest()
        {
            var config = new RewardConfig();

            var normal = ChapterRewardCalculator.CalculateChestReward(config, allThreeStars: false);
            var perfect = ChapterRewardCalculator.CalculateChestReward(config, allThreeStars: true);

            Assert.Greater(perfect.Coins, normal.Coins);
            Assert.Greater(perfect.Gems, normal.Gems);
        }
    }

    public class PlayerRankTests
    {
        [Test]
        public void ZeroXp_IsRankZero()
        {
            Assert.AreEqual(0, PlayerRank.RankForXp(0));
        }

        [Test]
        public void ExactThreshold_ReachesThatRank()
        {
            int xpForRank1 = PlayerRank.XpRequiredForRank(1);

            Assert.AreEqual(1, PlayerRank.RankForXp(xpForRank1));
        }

        [Test]
        public void OneXpBelowThreshold_StaysAtPreviousRank()
        {
            int xpForRank2 = PlayerRank.XpRequiredForRank(2);

            Assert.AreEqual(1, PlayerRank.RankForXp(xpForRank2 - 1));
        }

        [Test]
        public void RankRequirement_GrowsWithEachRank_NeverShrinks()
        {
            int previous = 0;
            for (int rank = 1; rank <= 20; rank++)
            {
                int required = PlayerRank.XpRequiredForRank(rank);
                Assert.Greater(required, previous);
                previous = required;
            }
        }

        [Test]
        public void NegativeXp_IsTreatedAsZero()
        {
            Assert.AreEqual(0, PlayerRank.RankForXp(-100));
        }
    }
}
