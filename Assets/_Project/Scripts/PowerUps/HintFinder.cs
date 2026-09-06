using GemGrid.Configuration;
using GemGrid.Core;
using GemGrid.Gameplay;

namespace GemGrid.PowerUps
{
    /// <summary>
    /// Hint power-up: finds one legal placement for a given shape, mirroring
    /// GameOverChecker's own brute-force style. Pure query — never mutates the grid;
    /// the caller (view layer) is responsible for only highlighting the returned cell.
    /// </summary>
    public static class HintFinder
    {
        public static Int2? FindValidPlacement(IGridModel grid, BlockShapeDefinition shape)
        {
            if (grid == null || shape == null) return null;

            for (int y = 0; y < grid.Height; y++)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    var origin = new Int2(x, y);
                    if (grid.CanPlace(shape, origin)) return origin;
                }
            }

            return null;
        }
    }
}
