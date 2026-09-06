using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GemGrid.SaveSystem
{
    /// <summary>
    /// Hand-rolled, dependency-free serializer for <see cref="SaveData"/> — deliberately
    /// not JSON/XML (no serialization library exists in this Unity project's package set,
    /// and pulling one in purely for a handful of ints/dictionaries would be a bigger risk
    /// than a small, fully unit-tested custom format). One "Key=Value" per line; list/dict
    /// fields use "|" between entries and ":" between an entry's own parts. Every value
    /// written is either an int/long or a level/power-up/achievement id — none of which can
    /// contain '=', '|', ':' or a newline — so no escaping is needed.
    /// </summary>
    public static class SaveSerializer
    {
        private const char LineBreak = '\n';
        private const char KeyValueSeparator = '=';
        private const char EntrySeparator = '|';
        private const char PartSeparator = ':';

        public static string Serialize(SaveData data)
        {
            var sb = new StringBuilder();
            AppendLine(sb, "Version", data.Version.ToString(CultureInfo.InvariantCulture));
            AppendLine(sb, "Coins", data.Coins.ToString(CultureInfo.InvariantCulture));
            AppendLine(sb, "Gems", data.Gems.ToString(CultureInfo.InvariantCulture));
            AppendLine(sb, "Energy", data.Energy.ToString(CultureInfo.InvariantCulture));
            AppendLine(sb, "LastEnergyRegenUnixSeconds", data.LastEnergyRegenUnixSeconds.ToString(CultureInfo.InvariantCulture));
            AppendLine(sb, "Xp", data.Xp.ToString(CultureInfo.InvariantCulture));
            AppendLine(sb, "DailyChallengeStreak", data.DailyChallengeStreak.ToString(CultureInfo.InvariantCulture));
            AppendLine(sb, "DailyChallengeLastCompletedDateUtc", data.DailyChallengeLastCompletedDateUtc ?? string.Empty);

            AppendLine(sb, "LevelStars", SerializeIntDictionary(data.LevelStars));
            AppendLine(sb, "PowerUpInventory", SerializeStringIntDictionary(data.PowerUpInventory));
            AppendLine(sb, "UnlockedAchievements", string.Join(EntrySeparator.ToString(), data.UnlockedAchievements ?? new List<string>()));

            return sb.ToString();
        }

        /// <summary>
        /// Never throws: any missing/corrupt line is silently skipped and the affected
        /// field keeps its <see cref="SaveData"/> default, so a partially-corrupt save
        /// still loads whatever it can instead of losing everything.
        /// </summary>
        public static SaveData Deserialize(string raw)
        {
            var data = new SaveData();
            if (string.IsNullOrEmpty(raw)) return data;

            foreach (var rawLine in raw.Split(LineBreak))
            {
                if (string.IsNullOrEmpty(rawLine)) continue;
                int separatorIndex = rawLine.IndexOf(KeyValueSeparator);
                if (separatorIndex < 0) continue;

                string key = rawLine.Substring(0, separatorIndex);
                string value = rawLine.Substring(separatorIndex + 1);

                try
                {
                    ApplyField(data, key, value);
                }
                catch
                {
                    // Malformed value for this one field — keep the default and keep
                    // parsing the rest of the file rather than failing the whole load.
                }
            }

            return data;
        }

        private static void ApplyField(SaveData data, string key, string value)
        {
            switch (key)
            {
                case "Version": data.Version = ParseInt(value); break;
                case "Coins": data.Coins = ParseInt(value); break;
                case "Gems": data.Gems = ParseInt(value); break;
                case "Energy": data.Energy = ParseInt(value); break;
                case "LastEnergyRegenUnixSeconds": data.LastEnergyRegenUnixSeconds = ParseLong(value); break;
                case "Xp": data.Xp = ParseInt(value); break;
                case "DailyChallengeStreak": data.DailyChallengeStreak = ParseInt(value); break;
                case "DailyChallengeLastCompletedDateUtc": data.DailyChallengeLastCompletedDateUtc = string.IsNullOrEmpty(value) ? null : value; break;
                case "LevelStars": data.LevelStars = DeserializeIntDictionary(value); break;
                case "PowerUpInventory": data.PowerUpInventory = DeserializeStringIntDictionary(value); break;
                case "UnlockedAchievements": data.UnlockedAchievements = DeserializeStringList(value); break;
            }
        }

        private static void AppendLine(StringBuilder sb, string key, string value)
        {
            sb.Append(key).Append(KeyValueSeparator).Append(value).Append(LineBreak);
        }

        private static int ParseInt(string value) => int.Parse(value, CultureInfo.InvariantCulture);
        private static long ParseLong(string value) => long.Parse(value, CultureInfo.InvariantCulture);

        private static string SerializeIntDictionary(Dictionary<int, int> dict)
        {
            if (dict == null || dict.Count == 0) return string.Empty;
            var parts = new List<string>(dict.Count);
            foreach (var kv in dict)
                parts.Add(kv.Key.ToString(CultureInfo.InvariantCulture) + PartSeparator + kv.Value.ToString(CultureInfo.InvariantCulture));
            return string.Join(EntrySeparator.ToString(), parts);
        }

        private static Dictionary<int, int> DeserializeIntDictionary(string raw)
        {
            var result = new Dictionary<int, int>();
            if (string.IsNullOrEmpty(raw)) return result;
            foreach (var entry in raw.Split(EntrySeparator))
            {
                if (string.IsNullOrEmpty(entry)) continue;
                var parts = entry.Split(PartSeparator);
                if (parts.Length != 2) continue;
                try { result[ParseInt(parts[0])] = ParseInt(parts[1]); }
                catch { /* skip malformed entry */ }
            }
            return result;
        }

        private static string SerializeStringIntDictionary(Dictionary<string, int> dict)
        {
            if (dict == null || dict.Count == 0) return string.Empty;
            var parts = new List<string>(dict.Count);
            foreach (var kv in dict)
                parts.Add(kv.Key + PartSeparator + kv.Value.ToString(CultureInfo.InvariantCulture));
            return string.Join(EntrySeparator.ToString(), parts);
        }

        private static Dictionary<string, int> DeserializeStringIntDictionary(string raw)
        {
            var result = new Dictionary<string, int>();
            if (string.IsNullOrEmpty(raw)) return result;
            foreach (var entry in raw.Split(EntrySeparator))
            {
                if (string.IsNullOrEmpty(entry)) continue;
                int idx = entry.LastIndexOf(PartSeparator);
                if (idx < 0) continue;
                try { result[entry.Substring(0, idx)] = ParseInt(entry.Substring(idx + 1)); }
                catch { /* skip malformed entry */ }
            }
            return result;
        }

        private static List<string> DeserializeStringList(string raw)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(raw)) return result;
            foreach (var entry in raw.Split(EntrySeparator))
                if (!string.IsNullOrEmpty(entry)) result.Add(entry);
            return result;
        }
    }
}
