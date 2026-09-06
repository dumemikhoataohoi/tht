using UnityEngine;
using UnityEngine.Events;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Exposes gameplay events as UnityEvents so animation/VFX can be wired from the
    /// Inspector without additional code. No animation clips/particles ship in M1 — this
    /// only provides the hook points; art arrives in M2.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see README_M1.md.
    /// </summary>
    [RequireComponent(typeof(GameManagerBehaviour))]
    public class GameplayAnimationHooks : MonoBehaviour
    {
        [SerializeField] private UnityEvent onBlockPlaced;
        [SerializeField] private UnityEvent onLinesCleared;
        [SerializeField] private UnityEvent onComboIncreased;
        [SerializeField] private UnityEvent onGameOver;
        [SerializeField] private UnityEvent onGameRestarted;

        private GameManagerBehaviour _gameManagerBehaviour;

        private void Awake() => _gameManagerBehaviour = GetComponent<GameManagerBehaviour>();

        private void Start()
        {
            var game = _gameManagerBehaviour.Game;
            game.BlockPlaced += OnBlockPlaced;
            game.LinesClearedEvent += OnLinesCleared;
            game.Combo.ComboChanged += OnComboChanged;
            game.GameOver += OnGameOver;
            game.GameRestarted += OnGameRestarted;
        }

        private void OnDestroy()
        {
            var game = _gameManagerBehaviour != null ? _gameManagerBehaviour.Game : null;
            if (game == null) return;
            game.BlockPlaced -= OnBlockPlaced;
            game.LinesClearedEvent -= OnLinesCleared;
            game.Combo.ComboChanged -= OnComboChanged;
            game.GameOver -= OnGameOver;
            game.GameRestarted -= OnGameRestarted;
        }

        private void OnBlockPlaced(BlockPlacedEventArgs args) => onBlockPlaced?.Invoke();
        private void OnLinesCleared(LinesClearedEventArgs args) => onLinesCleared?.Invoke();
        private void OnComboChanged(int combo) { if (combo > 1) onComboIncreased?.Invoke(); }
        private void OnGameOver() => onGameOver?.Invoke();
        private void OnGameRestarted() => onGameRestarted?.Invoke();
    }
}
