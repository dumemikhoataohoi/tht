namespace GemGrid.Gameplay
{
    /// <summary>
    /// Immutable capture of everything needed to fully restore a game-in-progress to an
    /// earlier point — used by the single-step Undo power-up (see GAME_DESIGN_V2.md §9).
    /// Deliberately excludes <see cref="GameManager.State"/>: Undo is only ever offered
    /// while still Playing, never used to escape Game Over — Continue is the dedicated
    /// mechanism for that (see GAME_DESIGN_V2.md §10).
    /// </summary>
    public readonly struct GameManagerSnapshot
    {
        public readonly bool[,] GridOccupancy;
        public readonly int Score;
        public readonly int CurrentCombo;
        public readonly int BestCombo;
        public readonly BlockData[] Tray;

        public GameManagerSnapshot(bool[,] gridOccupancy, int score, int currentCombo, int bestCombo, BlockData[] tray)
        {
            GridOccupancy = gridOccupancy;
            Score = score;
            CurrentCombo = currentCombo;
            BestCombo = bestCombo;
            Tray = tray;
        }
    }
}
