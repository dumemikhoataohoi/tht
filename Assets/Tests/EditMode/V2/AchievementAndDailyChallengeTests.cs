using System;
using GemGrid.Journey;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode.V2
{
    public class AchievementTrackerTests
    {
        [Test]
        public void FirstLevelComplete_UnlocksOnAnyWin()
        {
            var tracker = new AchievementTracker();
            AchievementId? unlocked = null;
            tracker.AchievementUnlocked += id => unlocked = id;

            tracker.OnLevelCompleted(stars: 1, totalLevelsCompleted: 1, chapterJustCompleted: false);

            Assert.IsTrue(tracker.IsUnlocked(AchievementId.FirstLevelComplete));
            Assert.AreEqual(AchievementId.FirstLevelComplete, unlocked);
        }

        [Test]
        public void ThreeStarAchievement_OnlyUnlocksOnAThreeStarWin()
        {
            var tracker = new AchievementTracker();

            tracker.OnLevelCompleted(stars: 2, totalLevelsCompleted: 1, chapterJustCompleted: false);
            Assert.IsFalse(tracker.IsUnlocked(AchievementId.FirstThreeStarLevel));

            tracker.OnLevelCompleted(stars: 3, totalLevelsCompleted: 2, chapterJustCompleted: false);
            Assert.IsTrue(tracker.IsUnlocked(AchievementId.FirstThreeStarLevel));
        }

        [Test]
        public void AlreadyUnlocked_DoesNotFireEventAgain()
        {
            var tracker = new AchievementTracker();
            tracker.OnLevelCompleted(1, 1, false);
            int fireCount = 0;
            tracker.AchievementUnlocked += _ => fireCount++;

            tracker.OnLevelCompleted(1, 2, false);

            Assert.AreEqual(0, fireCount);
        }

        [Test]
        public void PreviouslyUnlocked_LoadedFromSave_AreRespected()
        {
            var tracker = new AchievementTracker(new[] { AchievementId.FirstLevelComplete });

            Assert.IsTrue(tracker.IsUnlocked(AchievementId.FirstLevelComplete));
            Assert.IsFalse(tracker.IsUnlocked(AchievementId.TenLevelsComplete));
        }

        [Test]
        public void TenLevelsComplete_UnlocksAtThreshold()
        {
            var tracker = new AchievementTracker();

            tracker.OnLevelCompleted(1, totalLevelsCompleted: 9, chapterJustCompleted: false);
            Assert.IsFalse(tracker.IsUnlocked(AchievementId.TenLevelsComplete));

            tracker.OnLevelCompleted(1, totalLevelsCompleted: 10, chapterJustCompleted: false);
            Assert.IsTrue(tracker.IsUnlocked(AchievementId.TenLevelsComplete));
        }
    }

    public class DailyChallengeTests
    {
        [Test]
        public void SeedForDate_IsStableForSameDate_DifferentForDifferentDates()
        {
            var day1 = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc);
            var day1Again = new DateTime(2024, 5, 1, 12, 30, 0, DateTimeKind.Utc);
            var day2 = new DateTime(2024, 5, 2, 0, 0, 0, DateTimeKind.Utc);

            Assert.AreEqual(DailyChallengeSeed.SeedForDate(day1), DailyChallengeSeed.SeedForDate(day1Again));
            Assert.AreNotEqual(DailyChallengeSeed.SeedForDate(day1), DailyChallengeSeed.SeedForDate(day2));
        }

        [Test]
        public void RecordCompletion_ConsecutiveDays_IncrementsStreak()
        {
            var progress = new DailyChallengeProgress(null, 0);
            var day1 = new DateTime(2024, 5, 1, 10, 0, 0, DateTimeKind.Utc);
            var day2 = new DateTime(2024, 5, 2, 10, 0, 0, DateTimeKind.Utc);

            progress.RecordCompletion(day1);
            Assert.AreEqual(1, progress.Streak);

            progress.RecordCompletion(day2);
            Assert.AreEqual(2, progress.Streak);
        }

        [Test]
        public void RecordCompletion_SkippedDay_ResetsStreakToOne()
        {
            var progress = new DailyChallengeProgress(null, 0);
            var day1 = new DateTime(2024, 5, 1, 10, 0, 0, DateTimeKind.Utc);
            var day3 = new DateTime(2024, 5, 3, 10, 0, 0, DateTimeKind.Utc); // day 2 skipped

            progress.RecordCompletion(day1);
            progress.RecordCompletion(day3);

            Assert.AreEqual(1, progress.Streak);
        }

        [Test]
        public void RecordCompletion_TwiceSameDay_DoesNotDoubleCountStreak()
        {
            var progress = new DailyChallengeProgress(null, 0);
            var day1 = new DateTime(2024, 5, 1, 8, 0, 0, DateTimeKind.Utc);
            var day1Later = new DateTime(2024, 5, 1, 20, 0, 0, DateTimeKind.Utc);

            progress.RecordCompletion(day1);
            progress.RecordCompletion(day1Later);

            Assert.AreEqual(1, progress.Streak);
        }

        [Test]
        public void IsCompletedOn_ReflectsLastCompletedDate()
        {
            var progress = new DailyChallengeProgress(null, 0);
            var day1 = new DateTime(2024, 5, 1, 8, 0, 0, DateTimeKind.Utc);

            Assert.IsFalse(progress.IsCompletedOn(day1));
            progress.RecordCompletion(day1);
            Assert.IsTrue(progress.IsCompletedOn(day1));
        }
    }
}
