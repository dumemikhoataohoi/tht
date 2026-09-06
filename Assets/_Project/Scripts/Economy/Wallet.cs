using System;

namespace GemGrid.Economy
{
    /// <summary>
    /// Soft (Coins) and premium (Gems) currency balances. Pure C# — no persistence, no
    /// UnityEngine dependency; a higher-level coordinator reads/writes these values
    /// to/from <see cref="GemGrid.SaveSystem.SaveData"/>. Never allows a negative balance.
    /// </summary>
    public class Wallet
    {
        public int Coins { get; private set; }
        public int Gems { get; private set; }

        public event Action<int> CoinsChanged;
        public event Action<int> GemsChanged;

        public Wallet(int startingCoins = 0, int startingGems = 0)
        {
            Coins = Math.Max(0, startingCoins);
            Gems = Math.Max(0, startingGems);
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            Coins += amount;
            CoinsChanged?.Invoke(Coins);
        }

        public void AddGems(int amount)
        {
            if (amount <= 0) return;
            Gems += amount;
            GemsChanged?.Invoke(Gems);
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount < 0 || Coins < amount) return false;
            Coins -= amount;
            CoinsChanged?.Invoke(Coins);
            return true;
        }

        public bool TrySpendGems(int amount)
        {
            if (amount < 0 || Gems < amount) return false;
            Gems -= amount;
            GemsChanged?.Invoke(Gems);
            return true;
        }
    }
}
