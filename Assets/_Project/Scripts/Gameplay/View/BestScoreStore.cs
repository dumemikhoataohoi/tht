using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Persists the player's best score locally via <see cref="PlayerPrefs"/> — a
    /// trivial, built-in mechanism, deliberately NOT the full JSON save system planned
    /// for M7 (wallet/progression/settings with migration schema). Only "best score"
    /// belongs here; anything else should wait for the real Save module.
    /// </summary>
    public static class BestScoreStore
    {
        private const string Key = "GemGrid.BestScore";

        public static int Get() => PlayerPrefs.GetInt(Key, 0);

        /// <summary>Updates the stored best score if <paramref name="score"/> beats it. Returns the (possibly unchanged) best score.</summary>
        public static int SubmitScore(int score)
        {
            int best = Get();
            if (score > best)
            {
                best = score;
                PlayerPrefs.SetInt(Key, best);
                PlayerPrefs.Save();
            }
            return best;
        }
    }
}
