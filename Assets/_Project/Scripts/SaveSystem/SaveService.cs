using System;

namespace GemGrid.SaveSystem
{
    /// <summary>
    /// Load/save entry point. <see cref="Load"/> never throws — a missing, empty, or
    /// corrupt store (or an unexpected exception from a real Unity store) safely returns
    /// a fresh <see cref="SaveData"/> instead of crashing the game on startup.
    /// </summary>
    public class SaveService
    {
        private readonly ISaveStore _store;

        public SaveService(ISaveStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public SaveData Load()
        {
            try
            {
                string raw = _store.ReadRaw();
                if (string.IsNullOrEmpty(raw)) return new SaveData();
                return Migrate(SaveSerializer.Deserialize(raw));
            }
            catch
            {
                return new SaveData();
            }
        }

        public void Save(SaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            _store.WriteRaw(SaveSerializer.Serialize(data));
        }

        /// <summary>
        /// Version-gated migration hook. Currently a no-op (only version 1 exists) —
        /// this is where a future field rename/reshape gets translated forward so old
        /// saves keep working after an update.
        /// </summary>
        private static SaveData Migrate(SaveData data)
        {
            if (data.Version < 1) data.Version = 1;
            // Future: if (data.Version < 2) { ...translate v1 fields into v2 shape...; data.Version = 2; }
            return data;
        }
    }
}
