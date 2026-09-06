using System.Collections.Generic;
using GemGrid.Configuration;
using GemGrid.Core;

namespace GemGrid.Gameplay
{
    /// <summary>Pure cell-math helper shared by <see cref="GridModel"/> and game-over detection.</summary>
    public static class BlockPlacement
    {
        public static IEnumerable<Int2> GetOccupiedCells(BlockShapeDefinition shape, Int2 origin)
        {
            foreach (var offset in shape.Cells)
                yield return origin + offset;
        }
    }
}
