namespace GemGrid.Core
{
    /// <summary>No-op <see cref="IHapticService"/> used until a real device implementation exists.</summary>
    public sealed class NullHapticService : IHapticService
    {
        public static readonly NullHapticService Instance = new NullHapticService();

        public void Play(HapticStrength strength)
        {
        }
    }
}
