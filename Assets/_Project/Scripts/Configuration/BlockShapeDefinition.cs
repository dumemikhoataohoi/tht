using System;
using GemGrid.Core;

namespace GemGrid.Configuration
{
    /// <summary>
    /// Data-driven definition of one block shape: which cells (relative to an origin)
    /// it occupies and how often it should be spawned. Instances are authored inside a
    /// <see cref="BlockShapeSet"/> asset — never hard-coded in gameplay logic.
    /// </summary>
    [Serializable]
    public class BlockShapeDefinition
    {
        public string Id;
        public Int2[] Cells;
        public int SpawnWeight = 1;

        public BlockShapeDefinition()
        {
        }

        public BlockShapeDefinition(string id, Int2[] cells, int spawnWeight = 1)
        {
            Id = id;
            Cells = cells;
            SpawnWeight = spawnWeight;
        }

        public int CellCount => Cells?.Length ?? 0;
    }
}
