using System.Collections.Generic;
using GemGrid.Core;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Pure logic answering "is there any legal move left?" by brute-forcing every tray
    /// shape against every grid cell. Kept separate from <see cref="GameManager"/> so it
    /// can be tested in isolation.
    /// </summary>
    public static class GameOverChecker
    {
        public static bool HasAnyValidMove(IGridModel grid, IEnumerable<BlockData> trayBlocks)
        {
            foreach (var block in trayBlocks)
            {
                if (block.IsEmpty) continue;

                for (int y = 0; y < grid.Height; y++)
                {
                    for (int x = 0; x < grid.Width; x++)
                    {
                        if (grid.CanPlace(block.Shape, new Int2(x, y)))
                            return true;
                    }
                }
            }
            return false;
        }

        public static bool IsGameOver(IGridModel grid, IEnumerable<BlockData> trayBlocks) =>
            !HasAnyValidMove(grid, trayBlocks);
    }
}
