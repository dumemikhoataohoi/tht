using GemGrid.SaveSystem;
using UnityEngine;

namespace GemGrid.UI
{
    /// <summary>Thin UnityEngine.PlayerPrefs adapter for <see cref="ISaveStore"/> — same lightweight-persistence pattern already used by BestScoreStore, just for the full V2 save blob.</summary>
    public sealed class PlayerPrefsSaveStore : ISaveStore
    {
        private const string Key = "GemGrid.SaveData.V2";

        public string ReadRaw() => PlayerPrefs.GetString(Key, string.Empty);

        public void WriteRaw(string raw)
        {
            PlayerPrefs.SetString(Key, raw);
            PlayerPrefs.Save();
        }
    }
}
