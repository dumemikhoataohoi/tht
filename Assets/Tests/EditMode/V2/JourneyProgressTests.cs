using System.Collections.Generic;
using GemGrid.Journey;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode.V2
{
    public class JourneyProgressTests
    {
        [Test]
        public void Level0_IsAlwaysUnlocked_EvenWithNoProgress()
        {
            var progress = new JourneyProgress();

            Assert.IsTrue(progress.IsUnlocked(0));
        }

        [Test]
        public void Level1_IsLocked_UntilLevel0Completed()
        {
            var progress = new JourneyProgress();

            Assert.IsFalse(progress.IsUnlocked(1));

            progress.RecordResult(0, stars: 1);

            Assert.IsTrue(progress.IsUnlocked(1));
        }

        [Test]
        public void RecordResult_WithZeroStars_DoesNotUnlockNextLevel()
        {
            var progress = new JourneyProgress();

            progress.RecordResult(0, stars: 0); // a loss

            Assert.IsFalse(progress.IsUnlocked(1));
            Assert.IsFalse(progress.IsCompleted(0));
        }

        [Test]
        public void RecordResult_KeepsBestStarsAcrossReplays()
        {
            var progress = new JourneyProgress();

            progress.RecordResult(0, stars: 1);
            progress.RecordResult(0, stars: 3);
            progress.RecordResult(0, stars: 2); // worse replay must not overwrite the best

            Assert.AreEqual(3, progress.StarsFor(0));
        }

        [Test]
        public void IsChapterNowComplete_TrueOnlyWhenAllFifteenLevelsCompleted()
        {
            var progress = new JourneyProgress();
            for (int i = 0; i < LevelDefinition.LevelsPerChapter - 1; i++)
                progress.RecordResult(i, stars: 1);

            Assert.IsFalse(progress.IsChapterNowComplete(0));

            progress.RecordResult(LevelDefinition.LevelsPerChapter - 1, stars: 1);

            Assert.IsTrue(progress.IsChapterNowComplete(0));
        }

        [Test]
        public void IsChapterAllThreeStars_RequiresThreeStarsOnEveryLevel()
        {
            var progress = new JourneyProgress();
            for (int i = 0; i < LevelDefinition.LevelsPerChapter; i++)
                progress.RecordResult(i, stars: i == 0 ? 2 : 3);

            Assert.IsFalse(progress.IsChapterAllThreeStars(0));

            progress.RecordResult(0, stars: 3);

            Assert.IsTrue(progress.IsChapterAllThreeStars(0));
        }

        [Test]
        public void SnapshotAndLoadSnapshot_RoundTrips()
        {
            var progress = new JourneyProgress();
            progress.RecordResult(0, 3);
            progress.RecordResult(1, 2);

            var restored = new JourneyProgress();
            restored.LoadSnapshot(new List<KeyValuePair<int, int>>(progress.Snapshot()));

            Assert.AreEqual(3, restored.StarsFor(0));
            Assert.AreEqual(2, restored.StarsFor(1));
            Assert.IsTrue(restored.IsUnlocked(2));
        }

        [Test]
        public void TotalStars_SumsAcrossAllRecordedLevels()
        {
            var progress = new JourneyProgress();
            progress.RecordResult(0, 3);
            progress.RecordResult(1, 2);
            progress.RecordResult(2, 1);

            Assert.AreEqual(6, progress.TotalStars);
            Assert.AreEqual(3, progress.TotalLevelsCompleted);
        }
    }
}
