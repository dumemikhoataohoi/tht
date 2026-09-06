using System.Collections.Generic;

namespace GemGrid.SaveSystem
{
    /// <summary>
    /// Everything about a player's progress that must survive an app restart. Plain
    /// data only (no UnityEngine dependency) so it can be constructed/serialized/tested
    /// without a Unity Editor. <see cref="Version"/> drives <see cref="SaveService"/>'s
    /// migration hook — bump it whenever a field's meaning changes, never when only
    /// adding a new field with a safe default.
    /// </summary>
    public class SaveData
    {
        public const int CurrentVersion = 1;

        public int Version = CurrentVersion;

        // Economy.
        public int Coins;
        public int Gems;
        public int Energy;
        public long LastEnergyRegenUnixSeconds;

        // Journey progression: key = global level index (chapterIndex * levelsPerChapter
        // + levelIndexInChapter), value = best stars earned (>0 implies completed).
        public Dictionary<int, int> LevelStars = new Dictionary<int, int>();

        // Player rank.
        public int Xp;

        // Power-up inventory: key = PowerUpId.ToString() (kept as a plain string here so
        // SaveSystem never needs to reference GemGrid.PowerUps and stays a leaf module).
        public Dictionary<string, int> PowerUpInventory = new Dictionary<string, int>();

        // Achievements: id strings, same reasoning as PowerUpInventory above.
        public List<string> UnlockedAchievements = new List<string>();

        // Daily challenge.
        public string DailyChallengeLastCompletedDateUtc; // "yyyy-MM-dd" or null
        public int DailyChallengeStreak;
    }
}
