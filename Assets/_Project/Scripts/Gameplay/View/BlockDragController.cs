using GemGrid.Core;
using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Minimal single-finger drag-and-drop: a tray view (built in M2) calls
    /// <see cref="BeginDrag"/> when the player picks up a block; this component follows
    /// the pointer/touch (via <see cref="IPointerInputSource"/> — defaults to
    /// <see cref="UnityPointerInputSource"/>, which already supports Android touch) and,
    /// on release, resolves the target grid cell via <see cref="GridController"/> and
    /// calls <see cref="GameManager.TryPlaceBlock"/>. Visual feedback is intentionally
    /// bare — polish belongs to M2.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see README_M1.md.
    /// </summary>
    public class BlockDragController : MonoBehaviour
    {
        [SerializeField] private GridController gridController;
        [SerializeField] private Camera worldCamera;

        private GameManagerBehaviour _gameManagerBehaviour;
        private IPointerInputSource _inputSource = new UnityPointerInputSource();
        private int _draggedSlotIndex = -1;
        private Transform _draggedVisual;
        private Vector3 _dragStartPosition;
        private Vector3 _dragStartScale;

        private const float DragScaleMultiplier = 1.15f;

        /// <summary>Swap in a fake input source for tests, or a different real backend later.</summary>
        public void SetInputSource(IPointerInputSource inputSource) =>
            _inputSource = inputSource ?? new UnityPointerInputSource();

        private void Awake()
        {
            _gameManagerBehaviour = GameManagerBehaviour.Instance;
            if (_gameManagerBehaviour == null)
                throw new System.InvalidOperationException(
                    $"{nameof(BlockDragController)} requires the game to be bootstrapped from the Boot scene first " +
                    "(GameManagerBehaviour.Instance is null) — see UNITY_SETUP.md.");

            if (worldCamera == null) worldCamera = Camera.main;
        }

        /// <summary>Called by tray UI (M2) when the player starts dragging the block in <paramref name="traySlotIndex"/>.</summary>
        public void BeginDrag(int traySlotIndex, Transform visual)
        {
            _draggedSlotIndex = traySlotIndex;
            _draggedVisual = visual;
            _dragStartPosition = visual.position;
            _dragStartScale = visual.localScale;
            visual.localScale = _dragStartScale * DragScaleMultiplier;
        }

        private void Update()
        {
            if (_draggedSlotIndex < 0 || _draggedVisual == null) return;
            if (!_inputSource.TryGetPointer(out var pointer)) return;

            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(
                new Vector3(pointer.X, pointer.Y, -worldCamera.transform.position.z));
            worldPosition.z = 0f;
            _draggedVisual.position = worldPosition;

            if (pointer.Released)
                EndDrag(worldPosition);
        }

        private void EndDrag(Vector3 worldPosition)
        {
            var origin = gridController.WorldToGrid(worldPosition);
            bool placed = _gameManagerBehaviour.Game.TryPlaceBlock(_draggedSlotIndex, origin);

            // Invalid placement must not "lose" the block visually either — snap it
            // back to where the drag started, at its original (non-dragging) scale, so
            // the player can try again.
            if (!placed)
            {
                _draggedVisual.position = _dragStartPosition;
                _draggedVisual.localScale = _dragStartScale;
            }

            _draggedSlotIndex = -1;
            _draggedVisual = null;
        }
    }
}
