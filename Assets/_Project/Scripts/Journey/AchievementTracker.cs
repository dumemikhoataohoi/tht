using System;
using System.Collections.Generic;

namespace GemGrid.Journey
{
    /// <summary>
    /// Minimal one-shot achievement framework (see GAME_DESIGN_V2.md §14). Deliberately
    /// small — a handful of conditions checked directly against level-completion stats,
    /// not a generic condition-expression engine. Extend the switch/checks here as real
    /// achievement content is designed later.
    /// </summary>
    public sealed class AchievementTracker
    {
        private readonly HashSet<AchievementId> _unlocked;

        public event Action<AchievementId> AchievementUnlocked;

        public AchievementTracker(IEnumerable<AchievementId> alreadyUnlocked = null)
        {
            _unlocked = new HashSet<AchievementId>(alreadyUnlocked ?? Array.Empty<AchievementId>());
        }

        public IReadOnlyCollection<AchievementId> Unlocked => _unlocked;

        public bool IsUnlocked(AchievementId id) => _unlocked.Contains(id);

        public void OnLevelCompleted(int stars, int totalLevelsCompleted, bool chapterJustCompleted)
        {
            TryUnlock(AchievementId.FirstLevelComplete);
            if (stars >= 3) TryUnlock(AchievementId.FirstThreeStarLevel);
            if (chapterJustCompleted) TryUnlock(AchievementId.FirstChapterComplete);
            if (totalLevelsCompleted >= 10) TryUnlock(AchievementId.TenLevelsComplete);
        }

        private void TryUnlock(AchievementId id)
        {
            if (_unlocked.Add(id)) AchievementUnlocked?.Invoke(id);
        }
    }
}
