using System.Collections.Generic;
using System.Linq;

namespace GemGrid.Journey
{
    /// <summary>
    /// Unlock/star state for the whole Journey, addressed by <see cref="LevelDefinition.GlobalIndex"/>.
    /// Pure C# — a higher-level coordinator loads/saves its <see cref="Snapshot"/> via
    /// GemGrid.SaveSystem (this class never references that module directly, keeping
    /// Journey and SaveSystem decoupled per the implementation brief's module rules).
    /// </summary>
    public sealed class JourneyProgress
    {
        private readonly Dictionary<int, int> _starsByGlobalIndex = new Dictionary<int, int>();

        /// <summary>Level 0 is always unlocked; every other level unlocks once the one immediately before it is completed.</summary>
        public bool IsUnlocked(int globalIndex) => globalIndex == 0 || IsCompleted(globalIndex - 1);

        public bool IsCompleted(int globalIndex) => _starsByGlobalIndex.ContainsKey(globalIndex);

        public int StarsFor(int globalIndex) => _starsByGlobalIndex.TryGetValue(globalIndex, out var stars) ? stars : 0;

        public int TotalStars => _starsByGlobalIndex.Values.Sum();

        public int TotalLevelsCompleted => _starsByGlobalIndex.Count;

        /// <summary>Records a win. A loss (stars &lt;= 0) is not recorded — it never unlocks anything and never lowers a previous result.</summary>
        public void RecordResult(int globalIndex, int stars)
        {
            if (stars <= 0) return;
            if (!_starsByGlobalIndex.TryGetValue(globalIndex, out var best) || stars > best)
                _starsByGlobalIndex[globalIndex] = stars;
        }

        /// <summary>True the moment this result completes every level of its chapter for the first time — used to trigger the chapter Chest reward.</summary>
        public bool IsChapterNowComplete(int chapterIndex)
        {
            int start = chapterIndex * LevelDefinition.LevelsPerChapter;
            for (int i = 0; i < LevelDefinition.LevelsPerChapter; i++)
                if (!IsCompleted(start + i)) return false;
            return true;
        }

        public bool IsChapterAllThreeStars(int chapterIndex)
        {
            int start = chapterIndex * LevelDefinition.LevelsPerChapter;
            for (int i = 0; i < LevelDefinition.LevelsPerChapter; i++)
                if (StarsFor(start + i) < 3) return false;
            return true;
        }

        public IReadOnlyDictionary<int, int> Snapshot() => _starsByGlobalIndex;

        public void LoadSnapshot(IEnumerable<KeyValuePair<int, int>> snapshot)
        {
            _starsByGlobalIndex.Clear();
            if (snapshot == null) return;
            foreach (var kv in snapshot)
                if (kv.Value > 0) _starsByGlobalIndex[kv.Key] = kv.Value;
        }
    }
}
