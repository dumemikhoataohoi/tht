using System;

namespace GemGrid.Journey
{
    /// <summary>
    /// Data for one Journey level — the "LevelDefinition" data asset referenced in the
    /// implementation brief. Plain C# (no UnityEngine dependency) so <see cref="ChapterBuilder"/>
    /// and the objective/star systems stay fully unit-testable; wrapping instances in a
    /// ScriptableObject-backed asset for in-Editor authoring is a trivial follow-up once a
    /// Unity Editor is available (see README note in ChapterBuilder.cs).
    /// </summary>
    public sealed class LevelDefinition
    {
        public const int LevelsPerChapter = 15;

        public string LevelId { get; }
        public int ChapterIndex { get; }
        public int LevelIndexInChapter { get; }

        /// <summary>0-based index across the whole Journey (chapter-major order) — the stable id used for unlock/star persistence.</summary>
        public int GlobalIndex => ChapterIndex * LevelsPerChapter + LevelIndexInChapter;

        public ObjectiveDefinition PrimaryObjective { get; }

        /// <summary>Optional — completing it in addition to <see cref="PrimaryObjective"/> is what earns the 3rd star. Never required to clear the level.</summary>
        public ObjectiveDefinition BonusObjective { get; }

        /// <summary>0 = unlimited moves (the default — see GAME_DESIGN_V2.md §6: move-count timers are explicitly NOT a difficulty lever this game uses).</summary>
        public int MoveBudget { get; }

        /// <summary>Difficulty lever #2 — see GAME_DESIGN_V2.md §6. 0 = default shape mix, 1 = strongly favors larger/awkward shapes. Never fully removes small shapes (see BiasedBlockShapeProvider).</summary>
        public float ComplexityBias { get; }

        /// <summary>Difficulty lever #3 — see GAME_DESIGN_V2.md §6. Number of cells pre-occupied (but always normally clearable, never a permanent obstacle) at level start.</summary>
        public int PreFilledCellCount { get; }

        public LevelDefinition(int chapterIndex, int levelIndexInChapter, ObjectiveDefinition primaryObjective,
            ObjectiveDefinition bonusObjective, int moveBudget, float complexityBias, int preFilledCellCount)
        {
            if (chapterIndex < 0) throw new ArgumentOutOfRangeException(nameof(chapterIndex));
            if (levelIndexInChapter < 0 || levelIndexInChapter >= LevelsPerChapter)
                throw new ArgumentOutOfRangeException(nameof(levelIndexInChapter));

            ChapterIndex = chapterIndex;
            LevelIndexInChapter = levelIndexInChapter;
            LevelId = $"c{chapterIndex}_l{levelIndexInChapter}";
            PrimaryObjective = primaryObjective ?? throw new ArgumentNullException(nameof(primaryObjective));
            BonusObjective = bonusObjective;
            MoveBudget = Math.Max(0, moveBudget);
            ComplexityBias = Math.Clamp(complexityBias, 0f, 1f);
            PreFilledCellCount = Math.Max(0, preFilledCellCount);
        }
    }
}
