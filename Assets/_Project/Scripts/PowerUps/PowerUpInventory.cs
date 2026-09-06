using System.Collections.Generic;

namespace GemGrid.PowerUps
{
    /// <summary>Owned power-up counts. Pure data — a higher-level coordinator persists <see cref="Snapshot"/> via GemGrid.SaveSystem.</summary>
    public sealed class PowerUpInventory
    {
        private readonly Dictionary<PowerUpId, int> _owned = new Dictionary<PowerUpId, int>();

        public int GetOwnedCount(PowerUpId id) => _owned.TryGetValue(id, out var count) ? count : 0;

        public void Add(PowerUpId id, int amount)
        {
            if (amount <= 0) return;
            _owned[id] = GetOwnedCount(id) + amount;
        }

        /// <summary>Spends one owned unit. Returns false (and changes nothing) if none are owned — this is the "power-up legality" guard.</summary>
        public bool TryConsume(PowerUpId id)
        {
            int count = GetOwnedCount(id);
            if (count <= 0) return false;
            _owned[id] = count - 1;
            return true;
        }

        public IReadOnlyDictionary<PowerUpId, int> Snapshot() => _owned;

        public void LoadSnapshot(IEnumerable<KeyValuePair<PowerUpId, int>> snapshot)
        {
            _owned.Clear();
            if (snapshot == null) return;
            foreach (var kv in snapshot)
                if (kv.Value > 0) _owned[kv.Key] = kv.Value;
        }
    }
}
