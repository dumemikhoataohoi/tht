namespace GemGrid.Gameplay
{
    public readonly struct BlockPlacedEventArgs
    {
        public readonly int TraySlotIndex;
        public readonly int CellsPlaced;

        public BlockPlacedEventArgs(int traySlotIndex, int cellsPlaced)
        {
            TraySlotIndex = traySlotIndex;
            CellsPlaced = cellsPlaced;
        }
    }

    public readonly struct LinesClearedEventArgs
    {
        public readonly LineClearResult Result;
        public readonly int ComboCount;
        public readonly int ScoreAwarded;

        public LinesClearedEventArgs(LineClearResult result, int comboCount, int scoreAwarded)
        {
            Result = result;
            ComboCount = comboCount;
            ScoreAwarded = scoreAwarded;
        }
    }
}
