using System;

namespace GemGrid.Journey
{
    /// <summary>One objective's data: what to measure and the target to reach. Doubles as the "ObjectiveConfig" data referenced in the implementation brief — each level carries its own instance(s).</summary>
    public sealed class ObjectiveDefinition
    {
        public ObjectiveType Type { get; }
        public int Target { get; }

        public ObjectiveDefinition(ObjectiveType type, int target)
        {
            if (target <= 0) throw new ArgumentOutOfRangeException(nameof(target), "Objective target must be positive.");
            Type = type;
            Target = target;
        }
    }
}
