using System;

namespace GemGrid.Journey
{
    /// <summary>
    /// Generates a chapter's 15 <see cref="LevelDefinition"/>s from
    /// <see cref="DifficultyCurveConfig"/>'s formulas, instead of hand-authoring hundreds
    /// of level constants in C# (explicitly disallowed by the implementation brief).
    /// The first 3 levels of chapter 0 are the fixed onboarding sequence from
    /// GAME_DESIGN_V2.md §4 and bypass the curve entirely.
    /// </summary>
    public static class ChapterBuilder
    {
        public static LevelDefinition[] BuildChapter(int chapterIndex, DifficultyCurveConfig config)
        {
            if (chapterIndex < 0) throw new ArgumentOutOfRangeException(nameof(chapterIndex));
            if (config == null) throw new ArgumentNullException(nameof(config));

            var levels = new LevelDefinition[LevelDefinition.LevelsPerChapter];
            for (int i = 0; i < levels.Length; i++)
                levels[i] = BuildLevel(chapterIndex, i, config);
            return levels;
        }

        private static LevelDefinition BuildLevel(int chapterIndex, int levelIndexInChapter, DifficultyCurveConfig config)
        {
            if (chapterIndex == 0 && levelIndexInChapter == 0)
                return new LevelDefinition(chapterIndex, levelIndexInChapter,
                    new ObjectiveDefinition(ObjectiveType.FillSlot, 1), null, moveBudget: 0, complexityBias: 0f, preFilledCellCount: 0);

            if (chapterIndex == 0 && levelIndexInChapter == 1)
                return new LevelDefinition(chapterIndex, levelIndexInChapter,
                    new ObjectiveDefinition(ObjectiveType.LinesCleared, 1), null, moveBudget: 0, complexityBias: 0f, preFilledCellCount: 0);

            if (chapterIndex == 0 && levelIndexInChapter == 2)
                return new LevelDefinition(chapterIndex, levelIndexInChapter,
                    new ObjectiveDefinition(ObjectiveType.Score, 100), null, moveBudget: 0, complexityBias: 0f, preFilledCellCount: 0);

            int globalIndex = chapterIndex * LevelDefinition.LevelsPerChapter + levelIndexInChapter;

            var primary = BuildPrimaryObjective(globalIndex, levelIndexInChapter, config);
            var bonus = BuildBonusObjective(chapterIndex, levelIndexInChapter, config);
            float complexityBias = Math.Min(config.MaxComplexityBias, chapterIndex * config.ComplexityBiasPerChapter);
            int preFilled = BuildPreFilledCellCount(chapterIndex, levelIndexInChapter, config);

            return new LevelDefinition(chapterIndex, levelIndexInChapter, primary, bonus,
                moveBudget: 0, complexityBias, preFilled);
        }

        private static ObjectiveDefinition BuildPrimaryObjective(int globalIndex, int levelIndexInChapter, DifficultyCurveConfig config)
        {
            // Asymptotic growth: fast ramp early, slow ramp late — no sudden spikes.
            int scoreTarget = config.BaseScoreTarget +
                (int)(config.ScoreTargetGrowthRange * (1 - Math.Exp(-globalIndex / config.ScoreTargetGrowthRate)));

            // Rotate through objective types within a chapter so 15 levels don't all feel
            // identical, while every type still scales with the same score-derived target.
            switch (levelIndexInChapter % 5)
            {
                case 0: return new ObjectiveDefinition(ObjectiveType.Score, scoreTarget);
                case 1: return new ObjectiveDefinition(ObjectiveType.LinesCleared, Math.Max(2, scoreTarget / 60));
                case 2: return new ObjectiveDefinition(ObjectiveType.Score, scoreTarget);
                case 3: return new ObjectiveDefinition(ObjectiveType.Survive, Math.Max(6, scoreTarget / 25));
                default: return new ObjectiveDefinition(ObjectiveType.LinesCleared, Math.Max(2, scoreTarget / 60));
            }
        }

        private static ObjectiveDefinition BuildBonusObjective(int chapterIndex, int levelIndexInChapter, DifficultyCurveConfig config)
        {
            if (chapterIndex < config.MinChapterForBonusObjective) return null;

            int target = Math.Min(config.BonusObjectiveMaxTarget, config.BonusObjectiveBaseTarget + chapterIndex);
            var type = levelIndexInChapter % 2 == 0 ? ObjectiveType.Combo : ObjectiveType.MultiClear;
            return new ObjectiveDefinition(type, target);
        }

        private static int BuildPreFilledCellCount(int chapterIndex, int levelIndexInChapter, DifficultyCurveConfig config)
        {
            if (chapterIndex < config.MinChapterForPreFilledCells) return 0;
            if (levelIndexInChapter % config.PreFilledCellFrequency != config.PreFilledCellFrequency - 1) return 0;
            return config.PreFilledCellBaseCount + chapterIndex;
        }
    }
}
