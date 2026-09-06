using System;
using GemGrid.Configuration;
using GemGrid.Core;

namespace GemGrid.Gameplay
{
    /// <summary>Read/write model of the 8x8 (configurable) placement grid.</summary>
    public interface IGridModel
    {
        int Width { get; }
        int Height { get; }

        bool IsInside(Int2 cell);
        bool IsCellOccupied(Int2 cell);
        bool CanPlace(BlockShapeDefinition shape, Int2 origin);

        /// <summary>Places the shape if valid, clears any resulting full rows/columns, and returns the outcome.</summary>
        PlacementResult Place(BlockShapeDefinition shape, Int2 origin);

        void Reset();

        /// <summary>Clears a single occupied cell without checking/clearing any resulting
        /// full lines (used by the Hammer/Bomb power-ups). Returns false if the cell was
        /// out of bounds or already empty.</summary>
        bool ClearCell(Int2 cell);

        /// <summary>Full-grid occupancy snapshot for the single-step Undo power-up.</summary>
        bool[,] SnapshotOccupancy();

        /// <summary>Restores occupancy captured by <see cref="SnapshotOccupancy"/>. Does not
        /// re-run line-clear detection — the snapshot is assumed to already be a valid,
        /// previously-reached state.</summary>
        void RestoreOccupancy(bool[,] occupancy);

        event Action GridChanged;
        event Action<LineClearResult> LinesCleared;
    }
}
