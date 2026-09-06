using UnityEngine;

namespace GemGrid.Audio
{
    /// <summary>
    /// Generates short sine-wave <see cref="AudioClip"/>s procedurally at runtime.
    /// Copyright-safe placeholder SFX with zero external audio assets: every sound is
    /// pure math (see README_M1.md / LICENSE_MANIFEST.md for the no-copied-assets rule).
    /// Real sound design can replace <see cref="ProceduralToneAudioService"/> later
    /// without touching any gameplay code — it only depends on <see cref="IAudioService"/>.
    /// </summary>
    internal static class ToneGenerator
    {
        private const int SampleRate = 44100;
        private const float FadeSeconds = 0.01f;

        public static AudioClip CreateTone(string name, float frequencyHz, float durationSeconds, float volume)
        {
            int sampleCount = Mathf.RoundToInt(durationSeconds * SampleRate);
            var clip = AudioClip.Create(name, sampleCount, 1, SampleRate, false);

            var data = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)SampleRate;
                data[i] = Mathf.Sin(2f * Mathf.PI * frequencyHz * t) * volume * FadeEnvelope(t, durationSeconds);
            }

            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>Short linear fade in/out so the generated tone doesn't click at the edges.</summary>
        private static float FadeEnvelope(float t, float duration)
        {
            if (t < FadeSeconds) return t / FadeSeconds;
            if (t > duration - FadeSeconds) return Mathf.Max(0f, (duration - t) / FadeSeconds);
            return 1f;
        }
    }
}
