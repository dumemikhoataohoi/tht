namespace GemGrid.Core
{
    public enum HapticStrength
    {
        Light,
        Medium,
        Heavy
    }

    /// <summary>
    /// Haptic feedback hook point for M1. No real vibration implementation ships yet —
    /// see <see cref="NullHapticService"/> — a device-backed implementation can be
    /// added later without touching gameplay code.
    /// </summary>
    public interface IHapticService
    {
        void Play(HapticStrength strength);
    }
}
