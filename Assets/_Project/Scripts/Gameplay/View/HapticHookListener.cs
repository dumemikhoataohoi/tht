using GemGrid.Core;
using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Bridges gameplay events to <see cref="IHapticService"/>. Uses the no-op
    /// implementation by default; a real device vibration implementation can be wired in
    /// later via <see cref="SetHapticService"/> without touching gameplay code.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see README_M1.md.
    /// </summary>
    [RequireComponent(typeof(GameManagerBehaviour))]
    public class HapticHookListener : MonoBehaviour
    {
        private IHapticService _hapticService = NullHapticService.Instance;
        private GameManagerBehaviour _gameManagerBehaviour;

        public void SetHapticService(IHapticService hapticService) =>
            _hapticService = hapticService ?? NullHapticService.Instance;

        private void Awake() => _gameManagerBehaviour = GetComponent<GameManagerBehaviour>();

        private void Start()
        {
            var game = _gameManagerBehaviour.Game;
            game.BlockPlaced += OnBlockPlaced;
            game.LinesClearedEvent += OnLinesCleared;
            game.GameOver += OnGameOver;
        }

        private void OnDestroy()
        {
            var game = _gameManagerBehaviour != null ? _gameManagerBehaviour.Game : null;
            if (game == null) return;
            game.BlockPlaced -= OnBlockPlaced;
            game.LinesClearedEvent -= OnLinesCleared;
            game.GameOver -= OnGameOver;
        }

        private void OnBlockPlaced(BlockPlacedEventArgs args) => _hapticService.Play(HapticStrength.Light);
        private void OnLinesCleared(LinesClearedEventArgs args) => _hapticService.Play(HapticStrength.Medium);
        private void OnGameOver() => _hapticService.Play(HapticStrength.Heavy);
    }
}
