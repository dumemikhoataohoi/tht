namespace GemGrid.Audio
{
    /// <summary>No-op <see cref="IAudioService"/> used until real SFX/music assets exist.</summary>
    public sealed class NullAudioService : IAudioService
    {
        public static readonly NullAudioService Instance = new NullAudioService();

        public void PlaySfx(SfxId id)
        {
        }
    }
}
