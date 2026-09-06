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

        event Action GridChanged;
        event Action<LineClearResult> LinesCleared;
    }
}
