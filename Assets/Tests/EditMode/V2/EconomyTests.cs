using GemGrid.Economy;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode.V2
{
    public class WalletTests
    {
        [Test]
        public void TrySpendCoins_InsufficientBalance_FailsAndDoesNotChangeBalance()
        {
            var wallet = new Wallet(startingCoins: 10);

            bool spent = wallet.TrySpendCoins(20);

            Assert.IsFalse(spent);
            Assert.AreEqual(10, wallet.Coins);
        }

        [Test]
        public void TrySpendCoins_SufficientBalance_Succeeds()
        {
            var wallet = new Wallet(startingCoins: 50);

            Assert.IsTrue(wallet.TrySpendCoins(30));
            Assert.AreEqual(20, wallet.Coins);
        }

        [Test]
        public void AddCoins_NegativeOrZero_IsIgnored()
        {
            var wallet = new Wallet(startingCoins: 5);

            wallet.AddCoins(0);
            wallet.AddCoins(-10);

            Assert.AreEqual(5, wallet.Coins);
        }

        [Test]
        public void Wallet_NeverGoesNegative()
        {
            var wallet = new Wallet(startingCoins: -5, startingGems: -1);

            Assert.AreEqual(0, wallet.Coins);
            Assert.AreEqual(0, wallet.Gems);
        }

        [Test]
        public void TrySpendGems_ExactBalance_Succeeds()
        {
            var wallet = new Wallet(startingGems: 10);

            Assert.IsTrue(wallet.TrySpendGems(10));
            Assert.AreEqual(0, wallet.Gems);
        }
    }

    public class EnergyServiceTests
    {
        private sealed class FakeClock : IClock
        {
            public long Now;
            public long UtcNowUnixSeconds() => Now;
        }

        [Test]
        public void TrySpend_EntryCost_DecrementsByOne()
        {
            var clock = new FakeClock { Now = 1000 };
            var energy = new EnergyService(new EnergyConfig(), clock, startingEnergy: 5, lastRegenUnixSeconds: 1000);

            bool spent = energy.TrySpend(1);

            Assert.IsTrue(spent);
            Assert.AreEqual(4, energy.Current);
        }

        [Test]
        public void TrySpend_NotEnoughEnergy_Fails()
        {
            var clock = new FakeClock { Now = 1000 };
            var energy = new EnergyService(new EnergyConfig(), clock, startingEnergy: 0, lastRegenUnixSeconds: 1000);

            Assert.IsFalse(energy.TrySpend(1));
            Assert.AreEqual(0, energy.Current);
        }

        [Test]
        public void RegenerateFromElapsedTime_OneFullIntervalPassed_AddsOneEnergy()
        {
            var config = new EnergyConfig { MaxEnergy = 5, RegenIntervalSeconds = 60 };
            var clock = new FakeClock { Now = 0 };
            var energy = new EnergyService(config, clock, startingEnergy: 2, lastRegenUnixSeconds: 0);

            clock.Now = 60;
            energy.RegenerateFromElapsedTime();

            Assert.AreEqual(3, energy.Current);
        }

        [Test]
        public void RegenerateFromElapsedTime_MultipleIntervals_AddsMultipleButCapsAtMax()
        {
            var config = new EnergyConfig { MaxEnergy = 5, RegenIntervalSeconds = 60 };
            var clock = new FakeClock { Now = 0 };
            var energy = new EnergyService(config, clock, startingEnergy: 2, lastRegenUnixSeconds: 0);

            clock.Now = 60 * 10; // way more than enough to fully refill
            energy.RegenerateFromElapsedTime();

            Assert.AreEqual(5, energy.Current);
        }

        [Test]
        public void RegenerateFromElapsedTime_PartialInterval_DoesNotAddYet()
        {
            var config = new EnergyConfig { MaxEnergy = 5, RegenIntervalSeconds = 60 };
            var clock = new FakeClock { Now = 0 };
            var energy = new EnergyService(config, clock, startingEnergy: 2, lastRegenUnixSeconds: 0);

            clock.Now = 30;
            energy.RegenerateFromElapsedTime();

            Assert.AreEqual(2, energy.Current);
        }

        [Test]
        public void Add_FreeTopUp_NeverExceedsMax()
        {
            var config = new EnergyConfig { MaxEnergy = 5 };
            var clock = new FakeClock { Now = 0 };
            var energy = new EnergyService(config, clock, startingEnergy: 4, lastRegenUnixSeconds: 0);

            energy.Add(10);

            Assert.AreEqual(5, energy.Current);
        }
    }
}
