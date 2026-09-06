using GemGrid.Gameplay;

namespace GemGrid.PowerUps
{
    /// <summary>
    /// Thin wrapper around <see cref="GameManager.TryUndoLastMove"/> so the power-up
    /// system has one call site per power-up, consistent with Hammer/Shuffle/Hint. The
    /// single-step-history rule and the "only while Playing" guard both live on
    /// GameManager itself (see GameManagerSnapshot.cs) since Undo is fundamentally part
    /// of that class's own state, not something an external system can safely bolt on.
    /// </summary>
    public static class UndoAction
    {
        public static bool Apply(GameManager game) => game != null && game.TryUndoLastMove();
    }
}
