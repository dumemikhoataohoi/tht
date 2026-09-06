using System;

namespace GemGrid.Journey
{
    /// <summary>
    /// Running progress toward one <see cref="ObjectiveDefinition"/>, updated by
    /// <see cref="ObjectiveTracker"/> as the underlying GameManager fires its events.
    /// Each On* handler only reacts to the event type relevant to its own
    /// <see cref="Type"/>, so a single instance can be fed every event unconditionally.
    /// </summary>
    public sealed class ObjectiveProgress
    {
        public ObjectiveType Type { get; }
        public int Target { get; }
        public int Current { get; private set; }

        public bool IsComplete => Current >= Target;

        public ObjectiveProgress(ObjectiveDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            Type = definition.Type;
            Target = definition.Target;
        }

        public void OnScoreChanged(int totalScore)
        {
            if (Type != ObjectiveType.Score) return;
            Current = Math.Min(totalScore, Target);
        }

        public void OnComboChanged(int combo)
        {
            if (Type != ObjectiveType.Combo) return;
            Current = Math.Max(Current, Math.Min(combo, Target));
        }

        public void OnLinesCleared(int linesThisMove)
        {
            if (Type != ObjectiveType.LinesCleared) return;
            Current = Math.Min(Current + linesThisMove, Target);
        }

        public void OnMultiClear(int linesThisMove)
        {
            if (Type != ObjectiveType.MultiClear || linesThisMove < 2) return;
            Current = Math.Min(Current + 1, Target);
        }

        public void OnBlockPlaced()
        {
            if (Type != ObjectiveType.Survive) return;
            Current = Math.Min(Current + 1, Target);
        }

        public void OnTrayRefilled()
        {
            if (Type != ObjectiveType.FillSlot) return;
            Current = Math.Min(Current + 1, Target);
        }
    }
}
