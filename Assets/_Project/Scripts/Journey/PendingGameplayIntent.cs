namespace GemGrid.Journey
{
    /// <summary>
    /// Tiny static handoff between scenes: MainMenu/JourneyMap set which mode the next
    /// load of the Gameplay scene should start in; GameplaySessionStarter reads and
    /// clears it in Awake() (before any view component's Start() runs — see
    /// GameplaySessionStarter.cs for why that ordering matters). Deliberately not
    /// DontDestroyOnLoad/persisted state — it only needs to survive a single scene load,
    /// unlike GameManagerBehaviour.
    /// </summary>
    public static class PendingGameplayIntent
    {
        public static int? PendingJourneyGlobalLevelIndex { get; private set; }
        public static bool IsDailyChallenge { get; private set; }

        public static void RequestJourneyLevel(int globalLevelIndex)
        {
            PendingJourneyGlobalLevelIndex = globalLevelIndex;
            IsDailyChallenge = false;
        }

        public static void RequestDailyChallenge()
        {
            PendingJourneyGlobalLevelIndex = null;
            IsDailyChallenge = true;
        }

        public static void RequestClassic()
        {
            PendingJourneyGlobalLevelIndex = null;
            IsDailyChallenge = false;
        }

        /// <summary>Reads and clears the pending intent in one step, so it never leaks into a later, unrelated scene load.</summary>
        public static void Consume(out int? journeyGlobalLevelIndex, out bool isDaily)
        {
            journeyGlobalLevelIndex = PendingJourneyGlobalLevelIndex;
            isDaily = IsDailyChallenge;
            PendingJourneyGlobalLevelIndex = null;
            IsDailyChallenge = false;
        }
    }
}
