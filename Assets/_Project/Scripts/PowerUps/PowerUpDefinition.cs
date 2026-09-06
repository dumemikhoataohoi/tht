using System;

namespace GemGrid.PowerUps
{
    /// <summary>One power-up's economy data — cost and how many free uses each level grants before falling back to the owned inventory (GAME_DESIGN_V2.md §9).</summary>
    public sealed class PowerUpDefinition
    {
        public PowerUpId Id { get; }
        public int CoinCost { get; }
        public int FreeUsesPerLevel { get; }

        public PowerUpDefinition(PowerUpId id, int coinCost, int freeUsesPerLevel)
        {
            if (coinCost < 0) throw new ArgumentOutOfRangeException(nameof(coinCost));
            if (freeUsesPerLevel < 0) throw new ArgumentOutOfRangeException(nameof(freeUsesPerLevel));
            Id = id;
            CoinCost = coinCost;
            FreeUsesPerLevel = freeUsesPerLevel;
        }
    }
}
