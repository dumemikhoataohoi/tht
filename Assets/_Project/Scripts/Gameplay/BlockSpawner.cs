using System;
using System.Collections.Generic;
using GemGrid.Configuration;
using GemGrid.Core;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Owns the tray of blocks available to the player. Refills all slots at once
    /// (never one at a time) once every slot has been consumed, per GAME_DESIGN.md §3.
    /// </summary>
    public class BlockSpawner
    {
        private readonly IBlockShapeProvider _shapeProvider;
        private readonly IRandomSource _random;
        private readonly BlockData[] _tray;
        private int _nextInstanceId;

        public int TraySize { get; }
        public IReadOnlyList<BlockData> Tray => _tray;

        public event Action<IReadOnlyList<BlockData>> TrayRefilled;

        public BlockSpawner(IBlockShapeProvider shapeProvider, IRandomSource random, int traySize = 3)
        {
            _shapeProvider = shapeProvider ?? throw new ArgumentNullException(nameof(shapeProvider));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            if (traySize <= 0) throw new ArgumentOutOfRangeException(nameof(traySize));

            TraySize = traySize;
            _tray = new BlockData[traySize];
        }

        public bool IsTrayEmpty()
        {
            for (int i = 0; i < _tray.Length; i++)
                if (!_tray[i].IsEmpty) return false;
            return true;
        }

        public void RefillTray()
        {
            var library = _shapeProvider.GetLibrary();
            for (int i = 0; i < _tray.Length; i++)
            {
                var shape = library.GetRandomShape(_random);
                _tray[i] = new BlockData(GenerateInstanceId(), shape);
            }
            TrayRefilled?.Invoke(_tray);
        }

        public void ConsumeSlot(int index)
        {
            ValidateIndex(index);
            _tray[index] = BlockData.Empty;
        }

        public BlockData GetSlot(int index)
        {
            ValidateIndex(index);
            return _tray[index];
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= _tray.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        private string GenerateInstanceId() => $"block_{_nextInstanceId++}";
    }
}
