using System;

namespace GemGrid.Journey
{
    /// <summary>
    /// Daily Challenge completion/streak state (GAME_DESIGN_V2.md §13). Never tied to
    /// Gem Energy and never gates Journey — completing or skipping a day only affects
    /// this streak, nothing else.
    /// </summary>
    public sealed class DailyChallengeProgress
    {
        private const string DateFormat = "yyyy-MM-dd";

        public string LastCompletedDateUtc { get; private set; }
        public int Streak { get; private set; }

        public DailyChallengeProgress(string lastCompletedDateUtc, int streak)
        {
            LastCompletedDateUtc = lastCompletedDateUtc;
            Streak = Math.Max(0, streak);
        }

        public bool IsCompletedOn(DateTime utcNow) => LastCompletedDateUtc == Format(utcNow);

        /// <summary>Idempotent for the same day — calling it twice on the same UTC date does not double-count the streak.</summary>
        public void RecordCompletion(DateTime utcNow)
        {
            string today = Format(utcNow);
            if (LastCompletedDateUtc == today) return;

            string yesterday = Format(utcNow.AddDays(-1));
            Streak = (LastCompletedDateUtc == yesterday) ? Streak + 1 : 1;
            LastCompletedDateUtc = today;
        }

        private static string Format(DateTime d) => d.ToString(DateFormat);
    }
}
