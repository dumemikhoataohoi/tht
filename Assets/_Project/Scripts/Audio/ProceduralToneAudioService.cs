using System.Collections.Generic;
using UnityEngine;

namespace GemGrid.Audio
{
    /// <summary>
    /// Real (non-silent) <see cref="IAudioService"/> implementation for M3: plays a
    /// short procedurally-generated tone per <see cref="SfxId"/> via a single persistent
    /// <see cref="AudioSource"/>. No external audio assets — see <see cref="ToneGenerator"/>.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see UNITY_SETUP.md.
    /// </summary>
    public sealed class ProceduralToneAudioService : IAudioService
    {
        private readonly Dictionary<SfxId, AudioClip> _clips;
        private readonly AudioSource _source;

        public ProceduralToneAudioService()
        {
            _clips = new Dictionary<SfxId, AudioClip>
            {
                { SfxId.BlockPlace, ToneGenerator.CreateTone("sfx_block_place", 523.25f, 0.08f, 0.35f) },
                { SfxId.LineClear, ToneGenerator.CreateTone("sfx_line_clear", 783.99f, 0.18f, 0.40f) },
                { SfxId.Combo, ToneGenerator.CreateTone("sfx_combo", 1046.50f, 0.22f, 0.45f) },
                { SfxId.GameOver, ToneGenerator.CreateTone("sfx_game_over", 196.00f, 0.50f, 0.40f) },
                { SfxId.Restart, ToneGenerator.CreateTone("sfx_restart", 392.00f, 0.12f, 0.30f) },
                { SfxId.ButtonClick, ToneGenerator.CreateTone("sfx_button_click", 880.00f, 0.05f, 0.25f) },
            };

            var host = new GameObject("ProceduralAudioPlayback");
            Object.DontDestroyOnLoad(host);
            _source = host.AddComponent<AudioSource>();
            _source.playOnAwake = false;
        }

        public void PlaySfx(SfxId id)
        {
            if (_source == null) return;
            if (_clips.TryGetValue(id, out var clip) && clip != null)
                _source.PlayOneShot(clip);
        }
    }
}
