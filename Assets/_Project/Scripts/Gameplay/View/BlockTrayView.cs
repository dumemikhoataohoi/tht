using System.Collections.Generic;
using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Minimal visual for the 3-slot tray: one clickable/draggable square per occupied
    /// slot, laid out in a row below the grid. Each square is a plain placeholder — it
    /// represents "the block in this slot", not its actual multi-cell shape (the real
    /// footprint only becomes visible once placed on the grid, via
    /// <see cref="GridController"/>). Full per-shape tray art belongs to M2.
    ///
    /// Rebuilds itself whenever a block is consumed or the tray is refilled, by
    /// subscribing to <see cref="GameManager"/> events — never polls.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see README_M1.md.
    /// </summary>
    public class BlockTrayView : MonoBehaviour
    {
        [SerializeField] private BlockDragController dragController;
        [SerializeField] private Vector3 slotOrigin = new Vector3(0f, -2f, 0f);
        [SerializeField] private float slotSpacing = 1.5f;
        [SerializeField] private float slotSize = 0.8f;
        [SerializeField] private Color slotColor = new Color(1f, 0.65f, 0f); // orange

        private GameManagerBehaviour _gameManagerBehaviour;
        private readonly List<GameObject> _slotVisuals = new List<GameObject>();

        private void Awake()
        {
            _gameManagerBehaviour = GameManagerBehaviour.Instance;
            if (_gameManagerBehaviour == null)
                throw new System.InvalidOperationException(
                    $"{nameof(BlockTrayView)} requires the game to be bootstrapped from the Boot scene first " +
                    "(GameManagerBehaviour.Instance is null) — see UNITY_SETUP.md.");
        }

        private void Start()
        {
            var game = _gameManagerBehaviour.Game;
            game.BlockPlaced += OnTrayChanged;
            game.Spawner.TrayRefilled += OnTrayChanged;
            game.GameRestarted += OnTrayChanged;
            RebuildTray();
        }

        private void OnDestroy()
        {
            var game = _gameManagerBehaviour != null ? _gameManagerBehaviour.Game : null;
            if (game == null) return;
            game.BlockPlaced -= OnTrayChanged;
            game.Spawner.TrayRefilled -= OnTrayChanged;
            game.GameRestarted -= OnTrayChanged;
        }

        private void OnTrayChanged(BlockPlacedEventArgs _) => RebuildTray();
        private void OnTrayChanged(IReadOnlyList<BlockData> _) => RebuildTray();
        private void OnTrayChanged() => RebuildTray();

        private void RebuildTray()
        {
            foreach (var visual in _slotVisuals)
                if (visual != null) Destroy(visual);
            _slotVisuals.Clear();

            var spawner = _gameManagerBehaviour.Game.Spawner;
            for (int i = 0; i < spawner.TraySize; i++)
            {
                if (spawner.GetSlot(i).IsEmpty) continue;

                var slotGo = new GameObject($"TraySlot_{i}");
                slotGo.transform.SetParent(transform, false);
                slotGo.transform.localPosition = slotOrigin + new Vector3(i * slotSpacing, 0f, 0f);
                slotGo.transform.localScale = Vector3.one * slotSize;

                var renderer = slotGo.AddComponent<SpriteRenderer>();
                renderer.sprite = PlaceholderSprite.White;
                renderer.color = slotColor;

                var collider = slotGo.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;

                var blockView = slotGo.AddComponent<TrayBlockView>();
                blockView.SlotIndex = i;
                blockView.DragController = dragController;

                _slotVisuals.Add(slotGo);
            }
        }
    }
}
