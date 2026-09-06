using System;
using GemGrid.Core;
using GemGrid.Gameplay;

namespace GemGrid.Journey
{
    public enum LevelAttemptResult
    {
        InProgress,
        Won,
        Lost
    }

    /// <summary>
    /// Orchestrates one Journey level attempt on top of a freshly-built <see cref="GameManager"/>:
    /// seeds the "near-full" starting state (if any), tracks objectives, and resolves
    /// Win/Lose the same way Classic Mode resolves Game Over — by playing until no legal
    /// move remains (see GAME_DESIGN_V2.md §2: "kết thúc lượt = thất bại nếu chưa đạt mục
    /// tiêu... hoặc thành công nếu mục tiêu đã đạt trước khi hết nước đi"). This lets a
    /// player keep playing past completing the primary objective to also chase the bonus
    /// objective for a 3rd star, exactly like the design doc's star system implies.
    ///
    /// Pure C# — the <see cref="Game"/> instance is intentionally independent from
    /// GameManagerBehaviour.Instance (the persistent Classic-mode game); a thin Unity
    /// adapter is responsible for pointing the shared view layer (GridController etc.) at
    /// whichever GameManager is currently active. See GameManagerBehaviour.ReplaceGame.
    /// </summary>
    public sealed class JourneyLevelSession
    {
        public LevelDefinition Level { get; }
        public GameManager Game { get; }
        public ObjectiveTracker Objectives { get; }
        public LevelAttemptResult Result { get; private set; } = LevelAttemptResult.InProgress;

        public event Action<LevelAttemptResult> AttemptFinished;

        public JourneyLevelSession(LevelDefinition level, GameManager game)
        {
            Level = level ?? throw new ArgumentNullException(nameof(level));
            Game = game ?? throw new ArgumentNullException(nameof(game));
            Objectives = new ObjectiveTracker(game, level.PrimaryObjective, level.BonusObjective);
            Game.GameOver += OnGameOver;
        }

        public void Start()
        {
            Game.StartNewGame();

            if (Level.PreFilledCellCount > 0)
            {
                var seed = LevelSeeder.BuildPreFilledOccupancy(Game.Grid.Width, Game.Grid.Height, Level.PreFilledCellCount);
                Game.Grid.RestoreOccupancy(seed);
                Game.CheckGameOverNow();
            }
        }

        /// <summary>Unsubscribes from the GameManager — call when leaving the level (win, lose, or abandon) so this session can be garbage collected.</summary>
        public void Dispose()
        {
            Game.GameOver -= OnGameOver;
            Objectives.Detach();
        }

        public int CalculateStars() => StarCalculator.CalculateStars(
            Objectives.IsPrimaryComplete, Objectives.MovesUsedWhenPrimaryCompleted, Objectives.Bonus?.IsComplete ?? false);

        private void OnGameOver()
        {
            Result = Objectives.IsPrimaryComplete ? LevelAttemptResult.Won : LevelAttemptResult.Lost;
            AttemptFinished?.Invoke(Result);
        }
    }
}
