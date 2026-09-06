namespace GemGrid.SaveSystem
{
    /// <summary>
    /// Where the serialized save blob physically lives. Production uses a thin
    /// UnityEngine.PlayerPrefs-backed adapter (see Gameplay/View); tests use an
    /// in-memory fake — see the pattern already established by IRandomSource/IHapticService.
    /// </summary>
    public interface ISaveStore
    {
        /// <summary>Returns the raw saved string, or null/empty if nothing has been saved yet.</summary>
        string ReadRaw();

        void WriteRaw(string raw);
    }
}
