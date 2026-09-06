using System;
using System.Collections.Generic;
using GemGrid.Gameplay;

namespace GemGrid.Journey
{
    /// <summary>
    /// Wires a <see cref="GameManager"/>'s existing events to one primary and one
    /// optional bonus <see cref="ObjectiveProgress"/> — this is the runtime evaluation
    /// half of the level objective system (GAME_DESIGN_V2.md §7). Pure C#: it only
    /// depends on GameManager's own events, never mutates gameplay state.
    /// </summary>
    public sealed class ObjectiveTracker
    {
        private readonly GameManager _game;

        public ObjectiveProgress Primary { get; }
        public ObjectiveProgress Bonus { get; }

        /// <summary>Number of blocks successfully placed so far this attempt.</summary>
        public int MovesUsed { get; private set; }

        /// <summary>
        /// Moves used at the instant <see cref="Primary"/> first became complete, or -1 if
        /// not yet complete. Drives the 2-star "efficient clear" threshold — see
        /// <see cref="StarCalculator"/>.
        /// </summary>
        public int MovesUsedWhenPrimaryCompleted { get; private set; } = -1;

        public bool IsPrimaryComplete => Primary.IsComplete;

        public event Action ObjectiveProgressChanged;

        public ObjectiveTracker(GameManager game, ObjectiveDefinition primary, ObjectiveDefinition bonus)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));
            Primary = new ObjectiveProgress(primary ?? throw new ArgumentNullException(nameof(primary)));
            Bonus = bonus != null ? new ObjectiveProgress(bonus) : null;

            _game.Score.ScoreChanged += OnScoreChanged;
            _game.Combo.ComboChanged += OnComboChanged;
            _game.LinesClearedEvent += OnLinesCleared;
            _game.BlockPlaced += OnBlockPlaced;
            _game.Spawner.TrayRefilled += OnTrayRefilled;
        }

        /// <summary>Unsubscribes from the GameManager — call when the level attempt ends so this tracker can be garbage collected.</summary>
        public void Detach()
        {
            _game.Score.ScoreChanged -= OnScoreChanged;
            _game.Combo.ComboChanged -= OnComboChanged;
            _game.LinesClearedEvent -= OnLinesCleared;
            _game.BlockPlaced -= OnBlockPlaced;
            _game.Spawner.TrayRefilled -= OnTrayRefilled;
        }

        private void OnScoreChanged(int total) => Apply(p => p.OnScoreChanged(total));
        private void OnComboChanged(int combo) => Apply(p => p.OnComboChanged(combo));

        private void OnLinesCleared(LinesClearedEventArgs args)
        {
            int lines = args.Result.TotalLinesCleared;
            Apply(p => p.OnLinesCleared(lines));
            Apply(p => p.OnMultiClear(lines));
        }

        private void OnBlockPlaced(BlockPlacedEventArgs args)
        {
            MovesUsed++;
            Apply(p => p.OnBlockPlaced());
        }

        private void OnTrayRefilled(IReadOnlyList<BlockData> _) => Apply(p => p.OnTrayRefilled());

        private void Apply(Action<ObjectiveProgress> action)
        {
            action(Primary);
            if (Bonus != null) action(Bonus);

            if (MovesUsedWhenPrimaryCompleted < 0 && Primary.IsComplete)
                MovesUsedWhenPrimaryCompleted = MovesUsed;

            ObjectiveProgressChanged?.Invoke();
        }
    }
}
