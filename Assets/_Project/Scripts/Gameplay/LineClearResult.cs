using System;
using System.Collections.Generic;

namespace GemGrid.Gameplay
{
    /// <summary>Which rows/columns were cleared by a single <see cref="GridModel.Place"/> call.</summary>
    public readonly struct LineClearResult
    {
        public readonly IReadOnlyList<int> ClearedRows;
        public readonly IReadOnlyList<int> ClearedColumns;

        public LineClearResult(IReadOnlyList<int> clearedRows, IReadOnlyList<int> clearedColumns)
        {
            ClearedRows = clearedRows ?? Array.Empty<int>();
            ClearedColumns = clearedColumns ?? Array.Empty<int>();
        }

        public int TotalLinesCleared => ClearedRows.Count + ClearedColumns.Count;
        public bool HasAnyClear => TotalLinesCleared > 0;

        public static LineClearResult Empty { get; } = new LineClearResult(Array.Empty<int>(), Array.Empty<int>());
    }
}
