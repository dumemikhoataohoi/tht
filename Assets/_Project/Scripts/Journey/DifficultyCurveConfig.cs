namespace GemGrid.Journey
{
    /// <summary>
    /// The "DifficultyConfig" data referenced in the implementation brief: every tunable
    /// number <see cref="ChapterBuilder"/> uses to generate levels, kept out of that
    /// class's logic so balance can be retuned without touching code. Plain C# (no
    /// UnityEngine dependency); a ScriptableObject wrapper for in-Editor tuning is a
    /// trivial follow-up once a Unity Editor is available to author it.
    /// </summary>
    public class DifficultyCurveConfig
    {
        // Lever 1 — objective target growth (GAME_DESIGN_V2.md §6.1): approaches
        // BaseScoreTarget + ScoreTargetGrowthRange asymptotically as globalIndex grows,
        // so early levels ramp fast (visible progress) and late levels ramp slowly (no
        // sudden spikes).
        public int BaseScoreTarget = 150;
        public int ScoreTargetGrowthRange = 750;
        public double ScoreTargetGrowthRate = 20.0;

        // Lever 2 — shape complexity (GAME_DESIGN_V2.md §6.2): ComplexityBias handed to
        // BiasedBlockShapeProvider, increasing by chapter, capped so small shapes never
        // disappear entirely.
        public float ComplexityBiasPerChapter = 0.12f;
        public float MaxComplexityBias = 0.8f;

        // Lever 3 — near-full initial state (GAME_DESIGN_V2.md §6.3): only from
        // MinChapterForPreFilledCells onward, and only on 1-in-PreFilledCellFrequency
        // levels within a chapter.
        public int MinChapterForPreFilledCells = 2;
        public int PreFilledCellFrequency = 3;
        public int PreFilledCellBaseCount = 6;

        // Lever 4 — bonus objective for the 3rd star (GAME_DESIGN_V2.md §6.4): starts
        // from MinChapterForBonusObjective, target grows slowly with chapter.
        public int MinChapterForBonusObjective = 1;
        public int BonusObjectiveBaseTarget = 2;
        public int BonusObjectiveMaxTarget = 5;
    }
}
