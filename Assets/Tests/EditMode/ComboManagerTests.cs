using GemGrid.Configuration;
using GemGrid.Gameplay;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode
{
    public class ComboManagerTests
    {
        private static ComboRules DefaultRules() => new ComboRules
        {
            MultiplierPerComboStep = 0.1f,
            MaxMultiplier = 3.0f
        };

        [Test]
        public void RegisterMove_WithClear_IncrementsCurrentCombo()
        {
            var combo = new ComboManager(DefaultRules());

            combo.RegisterMove(didClearLines: true);

            Assert.AreEqual(1, combo.CurrentCombo);
        }

        [Test]
        public void RegisterMove_WithoutClear_ResetsCurrentComboToZero()
        {
            var combo = new ComboManager(DefaultRules());
            combo.RegisterMove(true);
            combo.RegisterMove(true);

            combo.RegisterMove(false);

            Assert.AreEqual(0, combo.CurrentCombo);
        }

        [Test]
        public void BestCombo_TracksHighestComboReachedInSession()
        {
            var combo = new ComboManager(DefaultRules());
            combo.RegisterMove(true);
            combo.RegisterMove(true);
            combo.RegisterMove(true);
            combo.RegisterMove(false);

            Assert.AreEqual(3, combo.BestCombo);
            Assert.AreEqual(0, combo.CurrentCombo);
        }

        [Test]
        public void CurrentMultiplier_IncreasesWithComboCount()
        {
            var combo = new ComboManager(DefaultRules());
            combo.RegisterMove(true);
            combo.RegisterMove(true);

            Assert.AreEqual(1.2f, combo.CurrentMultiplier, 0.0001f);
        }

        [Test]
        public void CurrentMultiplier_IsClampedToMaxMultiplier()
        {
            var combo = new ComboManager(new ComboRules { MultiplierPerComboStep = 1f, MaxMultiplier = 2f });
            for (int i = 0; i < 10; i++)
                combo.RegisterMove(true);

            Assert.AreEqual(2f, combo.CurrentMultiplier, 0.0001f);
        }

        [Test]
        public void Reset_SetsCurrentAndBestComboToZero()
        {
            var combo = new ComboManager(DefaultRules());
            combo.RegisterMove(true);
            combo.RegisterMove(true);

            combo.Reset();

            Assert.AreEqual(0, combo.CurrentCombo);
            Assert.AreEqual(0, combo.BestCombo);
        }
    }
}
