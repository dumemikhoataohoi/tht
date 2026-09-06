using System;
using GemGrid.Core;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Orchestrates one game session: wires grid, score, combo and spawner together and
    /// exposes the events that UI/Audio/Haptics/Animation hook into. Deliberately a plain
    /// C# class (not a MonoBehaviour) so the full gameplay loop is unit-testable; see
    /// <see cref="GameManagerBehaviour"/> for the Unity-facing composition root.
    /// </summary>
    public class GameManager
    {
        public IGridModel Grid { get; }
        public ScoreManager Score { get; }
        public ComboManager Combo { get; }
        public BlockSpawner Spawner { get; }

        public GameStateType State { get; private set; } = GameStateType.Idle;

        private GameManagerSnapshot? _lastMoveSnapshot;
        private bool _continueUsedThisSession;

        public event Action GameStarted;
        public event Action GameRestarted;
        public event Action<BlockPlacedEventArgs> BlockPlaced;
        public event Action<LinesClearedEventArgs> LinesClearedEvent;
        public event Action GameOver;
        public event Action<GameStateType> StateChanged;
        public event Action UndoApplied;
        public event Action ContinuedAfterGameOver;

        public GameManager(IGridModel grid, ScoreManager score, ComboManager combo, BlockSpawner spawner)
        {
            Grid = grid ?? throw new ArgumentNullException(nameof(grid));
            Score = score ?? throw new ArgumentNullException(nameof(score));
            Combo = combo ?? throw new ArgumentNullException(nameof(combo));
            Spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
        }

        public void StartNewGame()
        {
            Grid.Reset();
            Score.Reset();
            Combo.Reset();
            Spawner.RefillTray();
            _lastMoveSnapshot = null;
            _continueUsedThisSession = false;
            SetState(GameStateType.Playing);
            GameStarted?.Invoke();
            CheckGameOver();
        }

        public void RestartGame()
        {
            Grid.Reset();
            Score.Reset();
            Combo.Reset();
            Spawner.RefillTray();
            _lastMoveSnapshot = null;
            _continueUsedThisSession = false;
            SetState(GameStateType.Playing);
            GameRestarted?.Invoke();
            CheckGameOver();
        }

        /// <summary>Attempts to place the block sitting in <paramref name="traySlotIndex"/> at <paramref name="origin"/>.</summary>
        /// <returns>true if the placement was legal and applied; false otherwise (grid/state unchanged).</returns>
        public bool TryPlaceBlock(int traySlotIndex, Int2 origin)
        {
            if (State != GameStateType.Playing) return false;

            var block = Spawner.GetSlot(traySlotIndex);
            if (block.IsEmpty) return false;

            var preMoveSnapshot = CaptureSnapshot();

            var result = Grid.Place(block.Shape, origin);
            if (!result.Success) return false;

            _lastMoveSnapshot = preMoveSnapshot;

            Spawner.ConsumeSlot(traySlotIndex);
            Score.RegisterPlacement(result.CellsPlaced);
            BlockPlaced?.Invoke(new BlockPlacedEventArgs(traySlotIndex, result.CellsPlaced));

            bool didClear = result.Clears.HasAnyClear;
            Combo.RegisterMove(didClear);

            if (didClear)
            {
                float multiplier = Combo.CurrentMultiplier;
                int scoreBefore = Score.TotalScore;
                Score.RegisterClear(result.Clears.TotalLinesCleared, multiplier);
                int awarded = Score.TotalScore - scoreBefore;
                LinesClearedEvent?.Invoke(new LinesClearedEventArgs(result.Clears, Combo.CurrentCombo, awarded));
            }

            if (Spawner.IsTrayEmpty())
                Spawner.RefillTray();

            CheckGameOver();

            return true;
        }

        private void CheckGameOver()
        {
            if (State != GameStateType.Playing) return;

            if (!GameOverChecker.HasAnyValidMove(Grid, Spawner.Tray))
            {
                SetState(GameStateType.GameOver);
                GameOver?.Invoke();
            }
        }

        /// <summary>
        /// Re-runs Game Over detection outside the normal <see cref="TryPlaceBlock"/> flow —
        /// needed after anything else that can mutate the grid/tray while Playing (a
        /// Journey level seeding pre-filled cells at start, or the Hammer/Shuffle
        /// power-ups). Safe/no-op if not currently Playing. Returns whether the game is
        /// now in the GameOver state.
        /// </summary>
        public bool CheckGameOverNow()
        {
            CheckGameOver();
            return State == GameStateType.GameOver;
        }

        /// <summary>True while a single-step undo is available — see GAME_DESIGN_V2.md §9 ("Undo").</summary>
        public bool CanUndo => State == GameStateType.Playing && _lastMoveSnapshot.HasValue;

        /// <summary>
        /// Reverts the grid/score/combo/tray to how they were immediately before the last
        /// successful <see cref="TryPlaceBlock"/> call. Single-step only: using it clears
        /// the stored snapshot, matching the "1 bước gần nhất" limit in GAME_DESIGN_V2.md §9.
        /// </summary>
        public bool TryUndoLastMove()
        {
            if (!CanUndo) return false;

            RestoreSnapshot(_lastMoveSnapshot.Value);
            _lastMoveSnapshot = null;
            UndoApplied?.Invoke();
            return true;
        }

        /// <summary>True while a post-Game-Over continue is still available for this session — see GAME_DESIGN_V2.md §10.</summary>
        public bool CanContinue => State == GameStateType.GameOver && !_continueUsedThisSession;

        /// <summary>
        /// Journey/Daily "Continue" — gives the player one more chance after Game Over by
        /// freeing up space and dealing a fresh tray, then resumes Playing. Hard-capped at
        /// once per session (see <see cref="CanContinue"/>) regardless of which of the two
        /// payment paths (rewarded ad or Gems) the caller used — that choice is made by
        /// the UI/economy layer before calling this; this method only enforces the limit
        /// and performs the actual gameplay recovery.
        /// </summary>
        public bool TryContinueAfterGameOver()
        {
            if (!CanContinue) return false;

            _continueUsedThisSession = true;
            FreeUpSpaceDeterministically();
            Spawner.RefillTray();
            SetState(GameStateType.Playing);
            ContinuedAfterGameOver?.Invoke();
            CheckGameOver(); // defensive: FreeUpSpace + a fresh tray should already guarantee a legal move exists.

            return true;
        }

        private GameManagerSnapshot CaptureSnapshot() => new GameManagerSnapshot(
            Grid.SnapshotOccupancy(), Score.TotalScore, Combo.CurrentCombo, Combo.BestCombo, Spawner.SnapshotTray());

        private void RestoreSnapshot(GameManagerSnapshot snapshot)
        {
            Grid.RestoreOccupancy(snapshot.GridOccupancy);
            Score.RestoreTotalScore(snapshot.Score);
            Combo.RestoreState(snapshot.CurrentCombo, snapshot.BestCombo);
            Spawner.RestoreTray(snapshot.Tray);
        }

        /// <summary>
        /// Clears occupied cells in a fixed scan order (deterministic — not random, so
        /// behavior is reproducible/testable) until at least one legal move exists for the
        /// current tray, giving Continue genuine breathing room rather than a token
        /// gesture. Bounded by the grid's cell count so it can never loop forever.
        /// </summary>
        private void FreeUpSpaceDeterministically()
        {
            int safetyCap = Grid.Width * Grid.Height;
            int cleared = 0;

            for (int y = 0; y < Grid.Height && cleared < safetyCap; y++)
            {
                for (int x = 0; x < Grid.Width && cleared < safetyCap; x++)
                {
                    if (GameOverChecker.HasAnyValidMove(Grid, Spawner.Tray)) return;
                    if (Grid.ClearCell(new Int2(x, y))) cleared++;
                }
            }
        }

        private void SetState(GameStateType newState)
        {
            if (State == newState) return;
            State = newState;
            StateChanged?.Invoke(State);
        }
    }
}
