namespace GemGrid.Gameplay
{
    /// <summary>Outcome of attempting to place a block on the grid.</summary>
    public readonly struct PlacementResult
    {
        public readonly bool Success;
        public readonly int CellsPlaced;
        public readonly LineClearResult Clears;

        public PlacementResult(bool success, int cellsPlaced, LineClearResult clears)
        {
            Success = success;
            CellsPlaced = cellsPlaced;
            Clears = clears;
        }

        public static PlacementResult Failed { get; } = new PlacementResult(false, 0, LineClearResult.Empty);
    }
}
