namespace GemGrid.Economy
{
    /// <summary>
    /// Data for Gem Energy — see GAME_DESIGN_V2.md §8. Plain data class (no UnityEngine
    /// dependency) so it stays fully unit-testable; wrapping it as a ScriptableObject for
    /// in-Editor tuning is a trivial follow-up once a Unity Editor is available to author it.
    /// </summary>
    public class EnergyConfig
    {
        public int MaxEnergy = 5;
        public int RegenIntervalSeconds = 20 * 60; // 20 minutes per design doc §8.
        public int CostPerLevelEntry = 1;
    }
}
