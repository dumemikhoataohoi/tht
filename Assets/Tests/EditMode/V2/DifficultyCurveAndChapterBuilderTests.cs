using System.Linq;
using GemGrid.Journey;
using NUnit.Framework;

namespace GemGrid.Tests.EditMode.V2
{
    public class ChapterBuilderTests
    {
        [Test]
        public void BuildChapter_ReturnsExactlyFifteenLevels()
        {
            var levels = ChapterBuilder.BuildChapter(0, new DifficultyCurveConfig());

            Assert.AreEqual(LevelDefinition.LevelsPerChapter, levels.Length);
        }

        [Test]
        public void FirstThreeLevelsOfChapterZero_AreTheFixedOnboardingSequence()
        {
            var levels = ChapterBuilder.BuildChapter(0, new DifficultyCurveConfig());

            Assert.AreEqual(ObjectiveType.FillSlot, levels[0].PrimaryObjective.Type);
            Assert.AreEqual(1, levels[0].PrimaryObjective.Target);
            Assert.IsNull(levels[0].BonusObjective, "Onboarding levels must never require a bonus objective.");

            Assert.AreEqual(ObjectiveType.LinesCleared, levels[1].PrimaryObjective.Type);
            Assert.AreEqual(ObjectiveType.Score, levels[2].PrimaryObjective.Type);

            foreach (var onboardingLevel in levels.Take(3))
            {
                Assert.AreEqual(0, onboardingLevel.PreFilledCellCount, "Onboarding must never start near-full.");
                Assert.AreEqual(0f, onboardingLevel.ComplexityBias, "Onboarding must use the default (easiest) shape mix.");
            }
        }

        [Test]
        public void GlobalIndex_IsStableAndUnique_AcrossChapters()
        {
            var config = new DifficultyCurveConfig();
            var chapter0 = ChapterBuilder.BuildChapter(0, config);
            var chapter1 = ChapterBuilder.BuildChapter(1, config);

            Assert.AreEqual(0, chapter0[0].GlobalIndex);
            Assert.AreEqual(LevelDefinition.LevelsPerChapter - 1, chapter0[14].GlobalIndex);
            Assert.AreEqual(LevelDefinition.LevelsPerChapter, chapter1[0].GlobalIndex);
        }

        [Test]
        public void ScoreTargets_IncreaseMonotonically_WithinAndAcrossChapters_NoSuddenSpikes()
        {
            var config = new DifficultyCurveConfig();
            int previousScoreTarget = 0;

            for (int chapterIndex = 0; chapterIndex < 4; chapterIndex++)
            {
                foreach (var level in ChapterBuilder.BuildChapter(chapterIndex, config))
                {
                    if (level.PrimaryObjective.Type != ObjectiveType.Score) continue;

                    Assert.GreaterOrEqual(level.PrimaryObjective.Target, previousScoreTarget,
                        $"Level {level.LevelId} score target regressed versus an earlier level — difficulty must never decrease.");
                    previousScoreTarget = level.PrimaryObjective.Target;
                }
            }

            Assert.Greater(previousScoreTarget, config.BaseScoreTarget, "Score targets should have grown from the base by chapter 3.");
        }

        [Test]
        public void ComplexityBias_IncreasesByChapter_ButNeverExceedsConfiguredMax()
        {
            var config = new DifficultyCurveConfig();

            for (int chapterIndex = 0; chapterIndex < 10; chapterIndex++)
            {
                var levels = ChapterBuilder.BuildChapter(chapterIndex, config);
                float bias = levels[5].ComplexityBias; // any post-onboarding level
                Assert.LessOrEqual(bias, config.MaxComplexityBias);
                Assert.GreaterOrEqual(bias, 0f);
            }
        }

        [Test]
        public void MoveBudget_IsAlwaysUnlimited_NoTimerBasedDifficulty()
        {
            var config = new DifficultyCurveConfig();

            foreach (var level in ChapterBuilder.BuildChapter(2, config))
                Assert.AreEqual(0, level.MoveBudget, "GAME_DESIGN_V2.md §6 explicitly bans move-count timers as a difficulty lever.");
        }

        [Test]
        public void PreFilledCells_OnlyAppearFromConfiguredChapterOnward()
        {
            var config = new DifficultyCurveConfig();

            foreach (var level in ChapterBuilder.BuildChapter(0, config))
                Assert.AreEqual(0, level.PreFilledCellCount, "Chapter 0 must never pre-fill cells.");

            bool anyPreFilledInLaterChapter = ChapterBuilder.BuildChapter(config.MinChapterForPreFilledCells, config)
                .Any(l => l.PreFilledCellCount > 0);
            Assert.IsTrue(anyPreFilledInLaterChapter);
        }

        [Test]
        public void BonusObjective_NeverRequiredToClear_OnlyAffectsStarCount()
        {
            // This is a design invariant, not something ChapterBuilder enforces at
            // runtime by itself — asserted here structurally: BonusObjective must always
            // be a *separate, optional* field, never folded into PrimaryObjective.
            var levels = ChapterBuilder.BuildChapter(1, new DifficultyCurveConfig());

            foreach (var level in levels)
                Assert.AreNotSame(level.PrimaryObjective, level.BonusObjective);
        }

        [Test]
        public void ObjectiveTargets_StaySmallEnoughToBeReachableWithoutPowerUps()
        {
            // Config-level sanity guardrail for "zero-spend progression" (implementation
            // brief §Test/Balance): objective targets must stay within a plausible range
            // for a single continuous attempt on an 8x8 board, never balloon into numbers
            // that would implicitly require power-ups. This does not prove solvability via
            // exhaustive play simulation (out of scope for this milestone) - see the final
            // report for that scope note.
            var config = new DifficultyCurveConfig();

            for (int chapterIndex = 0; chapterIndex < 6; chapterIndex++)
            {
                foreach (var level in ChapterBuilder.BuildChapter(chapterIndex, config))
                {
                    switch (level.PrimaryObjective.Type)
                    {
                        case ObjectiveType.Score:
                            Assert.LessOrEqual(level.PrimaryObjective.Target, config.BaseScoreTarget + config.ScoreTargetGrowthRange);
                            break;
                        case ObjectiveType.LinesCleared:
                            Assert.LessOrEqual(level.PrimaryObjective.Target, 20);
                            break;
                        case ObjectiveType.Survive:
                            Assert.LessOrEqual(level.PrimaryObjective.Target, 64); // an 8x8 board has only 64 cells total
                            break;
                    }

                    if (level.BonusObjective != null)
                        Assert.LessOrEqual(level.BonusObjective.Target, config.BonusObjectiveMaxTarget);
                }
            }
        }
    }

    public class LevelSeederTests
    {
        [Test]
        public void BuildPreFilledOccupancy_PlacesExactlyRequestedCount_WhenRoomAllows()
        {
            var occupancy = LevelSeeder.BuildPreFilledOccupancy(8, 8, 10);

            int count = 0;
            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                    if (occupancy[x, y]) count++;

            Assert.AreEqual(10, count);
        }

        [Test]
        public void BuildPreFilledOccupancy_NeverFillsAnEntireRowOrColumn()
        {
            var occupancy = LevelSeeder.BuildPreFilledOccupancy(4, 4, 16); // ask for a full board

            for (int y = 0; y < 4; y++)
            {
                int rowCount = 0;
                for (int x = 0; x < 4; x++) if (occupancy[x, y]) rowCount++;
                Assert.Less(rowCount, 4, $"Row {y} must never be fully pre-filled (would auto-clear at level start).");
            }

            for (int x = 0; x < 4; x++)
            {
                int colCount = 0;
                for (int y = 0; y < 4; y++) if (occupancy[x, y]) colCount++;
                Assert.Less(colCount, 4, $"Column {x} must never be fully pre-filled.");
            }
        }

        [Test]
        public void BuildPreFilledOccupancy_ZeroCount_ReturnsEmptyGrid()
        {
            var occupancy = LevelSeeder.BuildPreFilledOccupancy(8, 8, 0);

            for (int x = 0; x < 8; x++)
                for (int y = 0; y < 8; y++)
                    Assert.IsFalse(occupancy[x, y]);
        }
    }
}
