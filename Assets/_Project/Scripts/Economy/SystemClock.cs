using System;

namespace GemGrid.Economy
{
    /// <summary>Production <see cref="IClock"/> backed by the real system clock (UTC, so it's DST/timezone-safe).</summary>
    public sealed class SystemClock : IClock
    {
        public long UtcNowUnixSeconds() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
