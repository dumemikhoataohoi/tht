using System;

namespace GemGrid.Journey
{
    /// <summary>Deterministic per-UTC-day seed so every player gets the same Daily Challenge board on a given date (GAME_DESIGN_V2.md §13).</summary>
    public static class DailyChallengeSeed
    {
        public static int SeedForDate(DateTime utcDate) => utcDate.Year * 10000 + utcDate.Month * 100 + utcDate.Day;
    }
}
