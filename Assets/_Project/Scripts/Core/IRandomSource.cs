namespace GemGrid.Core
{
    /// <summary>
    /// Abstraction over a random number source so gameplay logic (weighted block
    /// spawning) can be driven deterministically in unit tests.
    /// </summary>
    public interface IRandomSource
    {
        /// <summary>Returns an integer in [minInclusive, maxExclusive).</summary>
        int NextInt(int minInclusive, int maxExclusive);
    }
}
