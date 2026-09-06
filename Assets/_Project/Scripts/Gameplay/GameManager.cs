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

        public event Action GameStarted;
        public event Action GameRestarted;
        public event Action<BlockPlacedEventArgs> BlockPlaced;
        public event Action<LinesClearedEventArgs> LinesClearedEvent;
        public event Action GameOver;
        public event Action<GameStateType> StateChanged;

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

            var result = Grid.Place(block.Shape, origin);
            if (!result.Success) return false;

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

        private void SetState(GameStateType newState)
        {
            if (State == newState) return;
            State = newState;
            StateChanged?.Invoke(State);
        }
    }
}
