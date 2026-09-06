using System;
using System.Collections.Generic;

namespace GemGrid.PowerUps
{
    /// <summary>
    /// Per-attempt free-use tracker: each level grants <see cref="PowerUpDefinition.FreeUsesPerLevel"/>
    /// free uses (e.g. 1 free Hint, 1 free Undo) before falling back to the owned
    /// inventory — reset at the start of every level attempt. This is the "power-up
    /// legality" check requested in the implementation brief: <see cref="CanUse"/> answers
    /// whether a given use is legal without mutating anything; <see cref="Consume"/> is
    /// the only method that actually spends a free use or an owned unit.
    /// </summary>
    public sealed class PowerUpLevelUsage
    {
        private readonly PowerUpCatalog _catalog;
        private readonly Dictionary<PowerUpId, int> _freeUsesRemaining = new Dictionary<PowerUpId, int>();

        public PowerUpLevelUsage(PowerUpCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        }

        public void ResetForNewLevel() => _freeUsesRemaining.Clear();

        public bool CanUse(PowerUpId id, PowerUpInventory inventory) =>
            HasFreeUse(id) || inventory.GetOwnedCount(id) > 0;

        public bool Consume(PowerUpId id, PowerUpInventory inventory)
        {
            if (HasFreeUse(id))
            {
                _freeUsesRemaining[id] = RemainingFreeUses(id) - 1;
                return true;
            }
            return inventory.TryConsume(id);
        }

        private bool HasFreeUse(PowerUpId id) => RemainingFreeUses(id) > 0;

        private int RemainingFreeUses(PowerUpId id)
        {
            if (_freeUsesRemaining.TryGetValue(id, out var remaining)) return remaining;
            int initial = _catalog.Get(id).FreeUsesPerLevel;
            _freeUsesRemaining[id] = initial;
            return initial;
        }
    }
}
