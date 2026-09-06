using GemGrid.Gameplay;
using UnityEngine;

namespace GemGrid.Audio
{
    /// <summary>
    /// Bridges gameplay events to <see cref="IAudioService"/>. Defaults to the shared
    /// procedural-tone service (<see cref="SharedAudioService"/>) so SFX play with zero
    /// external audio assets; callers can still override via <see cref="SetAudioService"/>
    /// (e.g. tests use <see cref="NullAudioService"/>).
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see README_M1.md.
    /// </summary>
    public class AudioHookListener : MonoBehaviour
    {
        private IAudioService _audioService = SharedAudioService.Instance;
        private GameManagerBehaviour _gameManagerBehaviour;

        public void SetAudioService(IAudioService audioService) =>
            _audioService = audioService ?? NullAudioService.Instance;

        private void Awake()
        {
            _gameManagerBehaviour = GameManagerBehaviour.Instance;
            if (_gameManagerBehaviour == null)
                throw new System.InvalidOperationException(
                    $"{nameof(AudioHookListener)} requires the game to be bootstrapped from the Boot scene first " +
                    "(GameManagerBehaviour.Instance is null) — see UNITY_SETUP.md.");
        }

        private void Start()
        {
            var game = _gameManagerBehaviour.Game;
            game.BlockPlaced += OnBlockPlaced;
            game.LinesClearedEvent += OnLinesCleared;
            game.GameOver += OnGameOver;
            game.GameRestarted += OnGameRestarted;
            game.Combo.ComboChanged += OnComboChanged;
        }

        private void OnDestroy()
        {
            var game = _gameManagerBehaviour != null ? _gameManagerBehaviour.Game : null;
            if (game == null) return;
            game.BlockPlaced -= OnBlockPlaced;
            game.LinesClearedEvent -= OnLinesCleared;
            game.GameOver -= OnGameOver;
            game.GameRestarted -= OnGameRestarted;
            game.Combo.ComboChanged -= OnComboChanged;
        }

        private void OnBlockPlaced(BlockPlacedEventArgs args) => _audioService.PlaySfx(SfxId.BlockPlace);
        private void OnLinesCleared(LinesClearedEventArgs args) => _audioService.PlaySfx(SfxId.LineClear);
        private void OnGameOver() => _audioService.PlaySfx(SfxId.GameOver);
        private void OnGameRestarted() => _audioService.PlaySfx(SfxId.Restart);

        private void OnComboChanged(int combo)
        {
            if (combo > 1) _audioService.PlaySfx(SfxId.Combo);
        }
    }
}
