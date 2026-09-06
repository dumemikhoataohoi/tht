namespace GemGrid.Journey
{
    /// <summary>
    /// Builds the "near-full initial state" occupancy for <see cref="LevelDefinition.PreFilledCellCount"/>
    /// (difficulty lever #3 — GAME_DESIGN_V2.md §6.3). Deterministic (no randomness) and
    /// guarantees no row/column is ever fully pre-filled, so a level never opens with an
    /// instant "free" clear or, worse, an already-impossible board — pre-filled cells are
    /// always ordinary, clearable cells, never a permanent obstacle (GAME_DESIGN_V2.md §7
    /// explicitly bans obstacle cells as a difficulty lever).
    /// </summary>
    public static class LevelSeeder
    {
        public static bool[,] BuildPreFilledOccupancy(int width, int height, int count)
        {
            var occupancy = new bool[width, height];
            if (count <= 0) return occupancy;

            var rowCounts = new int[height];
            var colCounts = new int[width];
            int placed = 0;

            for (int y = 0; y < height && placed < count; y++)
            {
                for (int x = 0; x < width && placed < count; x++)
                {
                    if (rowCounts[y] >= width - 1) break; // never fill an entire row
                    if (colCounts[x] >= height - 1) continue; // never fill an entire column

                    occupancy[x, y] = true;
                    rowCounts[y]++;
                    colCounts[x]++;
                    placed++;
                }
            }

            return occupancy;
        }
    }
}
