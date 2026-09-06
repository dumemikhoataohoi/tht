namespace GemGrid.Core
{
    /// <summary>High-level game session state, per ARCHITECTURE.md §6 state machine.</summary>
    public enum GameStateType
    {
        Idle,
        Playing,
        Paused,
        GameOver
    }
}
