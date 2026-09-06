namespace GemGrid.Audio
{
    /// <summary>
    /// Lazily-created, process-wide <see cref="IAudioService"/> shared by
    /// <see cref="AudioHookListener"/> (gameplay SFX) and <see cref="ButtonClickSfx"/>
    /// (UI button SFX) so both use the same <see cref="ProceduralToneAudioService"/>
    /// instance/playback source instead of spawning one each.
    /// </summary>
    public static class SharedAudioService
    {
        private static IAudioService _instance;

        public static IAudioService Instance => _instance ??= new ProceduralToneAudioService();
    }
}
