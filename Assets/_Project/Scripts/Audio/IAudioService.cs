namespace GemGrid.Audio
{
    /// <summary>Audio hook point for M1. No real playback implementation ships yet — see <see cref="NullAudioService"/>.</summary>
    public interface IAudioService
    {
        void PlaySfx(SfxId id);
    }
}
