using System;
using System.Collections.Generic;

namespace GemGrid.PowerUps
{
    /// <summary>The "PowerUpConfig" data referenced in the implementation brief — every power-up's cost/free-use data in one place, not hard-coded at each call site.</summary>
    public sealed class PowerUpCatalog
    {
        private readonly Dictionary<PowerUpId, PowerUpDefinition> _byId;

        public PowerUpCatalog(IEnumerable<PowerUpDefinition> definitions)
        {
            _byId = new Dictionary<PowerUpId, PowerUpDefinition>();
            foreach (var definition in definitions)
                _byId[definition.Id] = definition;
        }

        public PowerUpDefinition Get(PowerUpId id) =>
            _byId.TryGetValue(id, out var definition) ? definition : throw new KeyNotFoundException($"No PowerUpDefinition for {id}.");

        /// <summary>Starting values per GAME_DESIGN_V2.md §9 — "khởi điểm, cân bằng lại sau playtest".</summary>
        public static PowerUpCatalog CreateDefault() => new PowerUpCatalog(new[]
        {
            new PowerUpDefinition(PowerUpId.Hint, 20, freeUsesPerLevel: 1),
            new PowerUpDefinition(PowerUpId.Hammer, 50, freeUsesPerLevel: 0),
            new PowerUpDefinition(PowerUpId.Shuffle, 60, freeUsesPerLevel: 0),
            new PowerUpDefinition(PowerUpId.Undo, 100, freeUsesPerLevel: 1),
            new PowerUpDefinition(PowerUpId.Bomb, 180, freeUsesPerLevel: 0),
            new PowerUpDefinition(PowerUpId.LineClear, 150, freeUsesPerLevel: 0),
            new PowerUpDefinition(PowerUpId.DoubleScore, 120, freeUsesPerLevel: 0),
        });
    }
}
