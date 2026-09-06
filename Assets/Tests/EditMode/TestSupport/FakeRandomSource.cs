using System.Collections.Generic;
using GemGrid.Core;

namespace GemGrid.Tests.EditMode
{
    /// <summary>Deterministic <see cref="IRandomSource"/> for tests: replays a fixed sequence, looping if exhausted.</summary>
    public class FakeRandomSource : IRandomSource
    {
        private readonly List<int> _sequence;
        private int _index;

        public FakeRandomSource(params int[] sequence)
        {
            _sequence = sequence.Length > 0 ? new List<int>(sequence) : new List<int> { 0 };
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            int value = _sequence[_index % _sequence.Count];
            _index++;
            int range = maxExclusive - minInclusive;
            return minInclusive + (value % range);
        }
    }
}
