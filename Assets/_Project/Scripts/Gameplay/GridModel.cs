using System;
using System.Collections.Generic;
using GemGrid.Configuration;
using GemGrid.Core;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Pure C# implementation of <see cref="IGridModel"/>. No UnityEngine dependency by
    /// design, so it can be unit tested with plain NUnit both inside and outside Unity.
    /// </summary>
    public class GridModel : IGridModel
    {
        private readonly bool[,] _occupied;

        public int Width { get; }
        public int Height { get; }

        public event Action GridChanged;
        public event Action<LineClearResult> LinesCleared;

        public GridModel(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
            _occupied = new bool[width, height];
        }

        public bool IsInside(Int2 cell) => cell.X >= 0 && cell.X < Width && cell.Y >= 0 && cell.Y < Height;

        public bool IsCellOccupied(Int2 cell)
        {
            if (!IsInside(cell)) throw new ArgumentOutOfRangeException(nameof(cell));
            return _occupied[cell.X, cell.Y];
        }

        public bool CanPlace(BlockShapeDefinition shape, Int2 origin)
        {
            if (shape == null) throw new ArgumentNullException(nameof(shape));

            foreach (var cell in BlockPlacement.GetOccupiedCells(shape, origin))
            {
                if (!IsInside(cell)) return false;
                if (_occupied[cell.X, cell.Y]) return false;
            }
            return true;
        }

        public PlacementResult Place(BlockShapeDefinition shape, Int2 origin)
        {
            if (!CanPlace(shape, origin)) return PlacementResult.Failed;

            int cellsPlaced = 0;
            foreach (var cell in BlockPlacement.GetOccupiedCells(shape, origin))
            {
                _occupied[cell.X, cell.Y] = true;
                cellsPlaced++;
            }

            GridChanged?.Invoke();

            var clears = ClearFullLines();
            if (clears.HasAnyClear)
                LinesCleared?.Invoke(clears);

            return new PlacementResult(true, cellsPlaced, clears);
        }

        public void Reset()
        {
            Array.Clear(_occupied, 0, _occupied.Length);
            GridChanged?.Invoke();
        }

        private LineClearResult ClearFullLines()
        {
            var fullRows = new List<int>();
            var fullColumns = new List<int>();

            for (int y = 0; y < Height; y++)
            {
                bool full = true;
                for (int x = 0; x < Width; x++)
                {
                    if (!_occupied[x, y]) { full = false; break; }
                }
                if (full) fullRows.Add(y);
            }

            for (int x = 0; x < Width; x++)
            {
                bool full = true;
                for (int y = 0; y < Height; y++)
                {
                    if (!_occupied[x, y]) { full = false; break; }
                }
                if (full) fullColumns.Add(x);
            }

            if (fullRows.Count == 0 && fullColumns.Count == 0)
                return LineClearResult.Empty;

            foreach (var y in fullRows)
                for (int x = 0; x < Width; x++)
                    _occupied[x, y] = false;

            foreach (var x in fullColumns)
                for (int y = 0; y < Height; y++)
                    _occupied[x, y] = false;

            GridChanged?.Invoke();

            return new LineClearResult(fullRows, fullColumns);
        }
    }
}
