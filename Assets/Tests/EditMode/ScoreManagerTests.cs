using GemGrid.Configuration;
using GemGrid.Gameplay;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode
{
    public class ScoreManagerTests
    {
        private static ScoreRules DefaultRules() => new ScoreRules
        {
            ScorePerPlacedCell = 1,
            BaseScorePerLine = 10,
            MultiLineBonusFactor = 0.5f
        };

        [Test]
        public void RegisterPlacement_AddsScoreProportionalToCellsPlaced()
        {
            var score = new ScoreManager(DefaultRules());

            score.RegisterPlacement(4);

            Assert.AreEqual(4, score.TotalScore);
        }

        [Test]
        public void RegisterClear_SingleLine_AddsBaseLineScore()
        {
            var score = new ScoreManager(DefaultRules());

            score.RegisterClear(1, comboMultiplier: 1f);

            Assert.AreEqual(10, score.TotalScore);
        }

        [Test]
        public void RegisterClear_MultipleLinesAtOnce_AppliesMultiLineBonus()
        {
            var score = new ScoreManager(DefaultRules());

            score.RegisterClear(2, comboMultiplier: 1f);

            // base 10 * 2 lines * (1 + 1 * 0.5) multiplier = 30
            Assert.AreEqual(30, score.TotalScore);
        }

        [Test]
        public void RegisterClear_WithComboMultiplier_ScalesScoreAccordingly()
        {
            var score = new ScoreManager(DefaultRules());

            score.RegisterClear(1, comboMultiplier: 2f);

            Assert.AreEqual(20, score.TotalScore);
        }

        [Test]
        public void RegisterClear_ZeroLines_DoesNotAddScore()
        {
            var score = new ScoreManager(DefaultRules());

            score.RegisterClear(0, comboMultiplier: 1f);

            Assert.AreEqual(0, score.TotalScore);
        }

        [Test]
        public void Reset_SetsTotalScoreBackToZero()
        {
            var score = new ScoreManager(DefaultRules());
            score.RegisterPlacement(10);

            score.Reset();

            Assert.AreEqual(0, score.TotalScore);
        }

        [Test]
        public void ScoreChanged_IsRaised_WithUpdatedTotal_WhenScoreIncreases()
        {
            var score = new ScoreManager(DefaultRules());
            int? observed = null;
            score.ScoreChanged += value => observed = value;

            score.RegisterPlacement(3);

            Assert.AreEqual(3, observed);
        }
    }
}
