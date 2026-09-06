namespace GemGrid.SaveSystem
{
    /// <summary>Trivial in-memory <see cref="ISaveStore"/> for tests and as a safe fallback.</summary>
    public class InMemorySaveStore : ISaveStore
    {
        private string _raw;

        public string ReadRaw() => _raw;

        public void WriteRaw(string raw) => _raw = raw;
    }
}
