using System;

namespace GemGrid.Core
{
    /// <summary>Production <see cref="IRandomSource"/> backed by <see cref="System.Random"/>.</summary>
    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly Random _random;

        public SystemRandomSource() => _random = new Random();

        public SystemRandomSource(int seed) => _random = new Random(seed);

        public int NextInt(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
    }
}
