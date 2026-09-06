namespace GemGrid.Economy
{
    /// <summary>Abstraction over "now", so <see cref="EnergyService"/>'s time-based regen is unit-testable.</summary>
    public interface IClock
    {
        long UtcNowUnixSeconds();
    }
}
