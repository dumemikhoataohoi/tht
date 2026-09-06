using System;
using System.Collections.Generic;
using GemGrid.Economy;
using GemGrid.Journey;
using GemGrid.PowerUps;
using GemGrid.SaveSystem;
using UnityEngine;

namespace GemGrid.UI
{
    /// <summary>
    /// Unity composition root for every V2 meta-progression system — Wallet, Gem Energy,
    /// power-up inventory, Journey unlock/star progress, achievements, and Daily
    /// Challenge streak — plus their persistence via GemGrid.SaveSystem. One instance
    /// lives for the whole app (DontDestroyOnLoad), created alongside GameManagerBehaviour
    /// at Boot (see GemGridAutoSetup/GemGridSceneSetup). Loads once on Awake; saves after
    /// every mutation that matters plus OnApplicationPause/Quit as a safety net.
    ///
    /// Deliberately independent from GameManagerBehaviour/GameManager: this service only
    /// ever deals in plain economy/progression data, never touches the grid/score/combo
    /// logic directly (Journey/PowerUps modules bridge the two where needed).
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see UNITY_SETUP.md.
    /// </summary>
    public class MetaProgressionService : MonoBehaviour
    {
        public static MetaProgressionService Instance { get; private set; }

        private SaveService _saveService;
        private SaveData _saveData;

        public Wallet Wallet { get; private set; }
        public EnergyService Energy { get; private set; }
        public PowerUpCatalog PowerUpCatalog { get; private set; }
        public PowerUpInventory PowerUpInventory { get; private set; }
        public PowerUpLevelUsage PowerUpLevelUsage { get; private set; }
        public JourneyProgress Journey { get; private set; }
        public AchievementTracker Achievements { get; private set; }
        public DailyChallengeProgress DailyChallenge { get; private set; }
        public RewardConfig RewardConfig { get; private set; }

        public int Xp { get; private set; }

        // Fully qualified: this class already has a property named "Journey", which would
        // otherwise shadow the GemGrid.Journey namespace for an unqualified reference.
        public int PlayerRankValue => GemGrid.Journey.PlayerRank.RankForXp(Xp);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            _saveService = new SaveService(new PlayerPrefsSaveStore());
            _saveData = _saveService.Load();

            Wallet = new Wallet(_saveData.Coins, _saveData.Gems);
            Energy = new EnergyService(new EnergyConfig(), new SystemClock(), _saveData.Energy, _saveData.LastEnergyRegenUnixSeconds);
            PowerUpCatalog = PowerUpCatalog.CreateDefault();
            PowerUpLevelUsage = new PowerUpLevelUsage(PowerUpCatalog);
            RewardConfig = new RewardConfig();
            Xp = _saveData.Xp;

            PowerUpInventory = new PowerUpInventory();
            PowerUpInventory.LoadSnapshot(DecodePowerUpInventory(_saveData.PowerUpInventory));

            Journey = new JourneyProgress();
            Journey.LoadSnapshot(_saveData.LevelStars);

            Achievements = new AchievementTracker(DecodeAchievements(_saveData.UnlockedAchievements));
            DailyChallenge = new DailyChallengeProgress(_saveData.DailyChallengeLastCompletedDateUtc, _saveData.DailyChallengeStreak);

            Wallet.CoinsChanged += _ => Save();
            Wallet.GemsChanged += _ => Save();
            Energy.EnergyChanged += _ => Save();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) Save();
        }

        private void OnApplicationQuit() => Save();

        /// <summary>
        /// Applies a finished Journey/Daily attempt's outcome: records stars/unlock,
        /// grants coins/xp/gems (and a chapter Chest the first time a chapter becomes
        /// fully complete), updates achievements, and persists. Call once per finished
        /// attempt (stars == 0 for a loss is safe/idempotent — see JourneyProgress.RecordResult).
        /// </summary>
        public LevelRewardResult RecordLevelResult(LevelDefinition level, int stars)
        {
            bool isFirstTimeClear = !Journey.IsCompleted(level.GlobalIndex);
            bool isFirstTimeThreeStar = stars == 3 && Journey.StarsFor(level.GlobalIndex) < 3;
            bool wasChapterCompleteBefore = Journey.IsChapterNowComplete(level.ChapterIndex);

            Journey.RecordResult(level.GlobalIndex, stars);

            var reward = LevelRewardCalculator.Calculate(RewardConfig, stars, isFirstTimeClear, isFirstTimeThreeStar);
            Wallet.AddCoins(reward.Coins);
            Wallet.AddGems(reward.Gems);
            AddXp(reward.Xp);

            bool isChapterCompleteNow = Journey.IsChapterNowComplete(level.ChapterIndex);
            bool chapterJustCompleted = !wasChapterCompleteBefore && isChapterCompleteNow;
            if (chapterJustCompleted)
            {
                var chest = ChapterRewardCalculator.CalculateChestReward(RewardConfig, Journey.IsChapterAllThreeStars(level.ChapterIndex));
                Wallet.AddCoins(chest.Coins);
                Wallet.AddGems(chest.Gems);
            }

            if (stars > 0)
                Achievements.OnLevelCompleted(stars, Journey.TotalLevelsCompleted, chapterJustCompleted);

            Save();
            return reward;
        }

        public void AddXp(int amount)
        {
            if (amount <= 0) return;
            Xp += amount;
            Save();
        }

        public void RecordDailyChallengeCompletion()
        {
            DailyChallenge.RecordCompletion(DateTime.UtcNow);
            Save();
        }

        public void Save()
        {
            _saveData.Coins = Wallet.Coins;
            _saveData.Gems = Wallet.Gems;
            _saveData.Energy = Energy.Current;
            _saveData.LastEnergyRegenUnixSeconds = Energy.LastRegenUnixSeconds;
            _saveData.Xp = Xp;
            _saveData.LevelStars = new Dictionary<int, int>(Journey.Snapshot());
            _saveData.PowerUpInventory = EncodePowerUpInventory(PowerUpInventory.Snapshot());
            _saveData.UnlockedAchievements = EncodeAchievements(Achievements.Unlocked);
            _saveData.DailyChallengeLastCompletedDateUtc = DailyChallenge.LastCompletedDateUtc;
            _saveData.DailyChallengeStreak = DailyChallenge.Streak;

            _saveService.Save(_saveData);
        }

        private static Dictionary<PowerUpId, int> DecodePowerUpInventory(Dictionary<string, int> raw)
        {
            var result = new Dictionary<PowerUpId, int>();
            if (raw == null) return result;
            foreach (var kv in raw)
                if (Enum.TryParse(kv.Key, out PowerUpId id) && kv.Value > 0) result[id] = kv.Value;
            return result;
        }

        private static Dictionary<string, int> EncodePowerUpInventory(IReadOnlyDictionary<PowerUpId, int> snapshot)
        {
            var result = new Dictionary<string, int>();
            foreach (var kv in snapshot)
                result[kv.Key.ToString()] = kv.Value;
            return result;
        }

        private static List<AchievementId> DecodeAchievements(List<string> raw)
        {
            var result = new List<AchievementId>();
            if (raw == null) return result;
            foreach (var entry in raw)
                if (Enum.TryParse(entry, out AchievementId id)) result.Add(id);
            return result;
        }

        private static List<string> EncodeAchievements(IReadOnlyCollection<AchievementId> unlocked)
        {
            var result = new List<string>();
            foreach (var id in unlocked)
                result.Add(id.ToString());
            return result;
        }
    }
}
