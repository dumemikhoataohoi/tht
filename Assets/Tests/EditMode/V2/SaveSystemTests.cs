using GemGrid.SaveSystem;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode.V2
{
    public class SaveSystemTests
    {
        [Test]
        public void SaveService_Load_WithNothingSaved_ReturnsFreshDefaults()
        {
            var service = new SaveService(new InMemorySaveStore());

            var data = service.Load();

            Assert.AreEqual(SaveData.CurrentVersion, data.Version);
            Assert.AreEqual(0, data.Coins);
            Assert.AreEqual(0, data.Gems);
            Assert.IsEmpty(data.LevelStars);
        }

        [Test]
        public void SaveService_SaveThenLoad_RoundTripsAllFields()
        {
            var store = new InMemorySaveStore();
            var service = new SaveService(store);
            var data = new SaveData
            {
                Coins = 340,
                Gems = 12,
                Energy = 3,
                LastEnergyRegenUnixSeconds = 1234567890,
                Xp = 250,
                DailyChallengeStreak = 4,
                DailyChallengeLastCompletedDateUtc = "2024-05-01",
            };
            data.LevelStars[0] = 3;
            data.LevelStars[14] = 1;
            data.PowerUpInventory["Hint"] = 2;
            data.PowerUpInventory["Hammer"] = 1;
            data.UnlockedAchievements.Add("FirstLevelComplete");

            service.Save(data);
            var loaded = service.Load();

            Assert.AreEqual(340, loaded.Coins);
            Assert.AreEqual(12, loaded.Gems);
            Assert.AreEqual(3, loaded.Energy);
            Assert.AreEqual(1234567890, loaded.LastEnergyRegenUnixSeconds);
            Assert.AreEqual(250, loaded.Xp);
            Assert.AreEqual(4, loaded.DailyChallengeStreak);
            Assert.AreEqual("2024-05-01", loaded.DailyChallengeLastCompletedDateUtc);
            Assert.AreEqual(3, loaded.LevelStars[0]);
            Assert.AreEqual(1, loaded.LevelStars[14]);
            Assert.AreEqual(2, loaded.PowerUpInventory["Hint"]);
            Assert.AreEqual(1, loaded.PowerUpInventory["Hammer"]);
            CollectionAssert.Contains(loaded.UnlockedAchievements, "FirstLevelComplete");
        }

        [Test]
        public void SaveService_Load_WithCorruptData_SafelyReturnsDefaults_NoException()
        {
            var store = new InMemorySaveStore();
            store.WriteRaw("this is not a valid save blob \0\0\0 %%% ===");
            var service = new SaveService(store);

            SaveData data = null;
            Assert.DoesNotThrow(() => data = service.Load());
            Assert.IsNotNull(data);
        }

        [Test]
        public void SaveService_Load_WhenStoreThrows_SafelyReturnsDefaults()
        {
            var service = new SaveService(new ThrowingSaveStore());

            SaveData data = null;
            Assert.DoesNotThrow(() => data = service.Load());
            Assert.IsNotNull(data);
        }

        [Test]
        public void SaveSerializer_Deserialize_PartiallyCorruptLines_KeepsWhatItCanParse()
        {
            string raw = "Coins=100\nGems=not_a_number\nXp=50\n";

            var data = SaveSerializer.Deserialize(raw);

            Assert.AreEqual(100, data.Coins);
            Assert.AreEqual(0, data.Gems); // corrupt line skipped, default kept
            Assert.AreEqual(50, data.Xp);
        }

        private sealed class ThrowingSaveStore : ISaveStore
        {
            public string ReadRaw() => throw new System.InvalidOperationException("simulated storage failure");
            public void WriteRaw(string raw) => throw new System.InvalidOperationException("simulated storage failure");
        }
    }
}
