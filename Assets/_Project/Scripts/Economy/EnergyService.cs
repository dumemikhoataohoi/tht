using System;

namespace GemGrid.Economy
{
    /// <summary>
    /// Gem Energy — gates Journey level *entry* only (see GAME_DESIGN_V2.md §8). Classic
    /// Mode must never construct/consult this service at all; that separation is enforced
    /// by callers, not by this class (there is nothing Classic-specific to guard here).
    ///
    /// Regenerates for free over real elapsed time, even while the app is closed —
    /// <see cref="RegenerateFromElapsedTime"/> is safe to call as often as needed (e.g. on
    /// every UI refresh) and only actually advances state when a full interval has passed.
    /// </summary>
    public class EnergyService
    {
        private readonly EnergyConfig _config;
        private readonly IClock _clock;

        public int Current { get; private set; }
        public long LastRegenUnixSeconds { get; private set; }

        public event Action<int> EnergyChanged;

        public EnergyService(EnergyConfig config, IClock clock, int startingEnergy, long lastRegenUnixSeconds)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            Current = Clamp(startingEnergy);
            LastRegenUnixSeconds = lastRegenUnixSeconds;
        }

        public bool IsFull => Current >= _config.MaxEnergy;

        public void RegenerateFromElapsedTime()
        {
            long now = _clock.UtcNowUnixSeconds();

            if (IsFull)
            {
                // Don't let a full bank accumulate "banked" regen ticks while capped —
                // reset the anchor to now so regen starts counting the instant Energy is
                // spent below the cap again.
                LastRegenUnixSeconds = now;
                return;
            }

            long elapsed = now - LastRegenUnixSeconds;
            if (elapsed < _config.RegenIntervalSeconds) return;

            int ticks = (int)(elapsed / _config.RegenIntervalSeconds);
            if (ticks <= 0) return;

            int before = Current;
            Current = Clamp(Current + ticks);
            LastRegenUnixSeconds += (long)ticks * _config.RegenIntervalSeconds;

            if (Current != before) EnergyChanged?.Invoke(Current);
        }

        public bool HasEnough(int amount)
        {
            RegenerateFromElapsedTime();
            return Current >= amount;
        }

        /// <summary>Spends Energy (e.g. entering a Journey level). Never called on failure/retry — see GAME_DESIGN_V2.md §8.</summary>
        public bool TrySpend(int amount)
        {
            RegenerateFromElapsedTime();
            if (amount <= 0 || Current < amount) return false;
            Current -= amount;
            EnergyChanged?.Invoke(Current);
            return true;
        }

        /// <summary>Free top-up (rewarded ad, rank-up, daily challenge reward, ...).</summary>
        public void Add(int amount)
        {
            if (amount <= 0) return;
            int before = Current;
            Current = Clamp(Current + amount);
            if (Current != before) EnergyChanged?.Invoke(Current);
        }

        private int Clamp(int value) => Math.Max(0, Math.Min(_config.MaxEnergy, value));
    }
}
