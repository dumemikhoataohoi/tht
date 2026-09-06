using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Minimal single-finger drag-and-drop: a tray view (built in M2) calls
    /// <see cref="BeginDrag"/> when the player picks up a block; this component follows
    /// the pointer/touch and, on release, resolves the target grid cell via
    /// <see cref="GridController"/> and calls <see cref="GameManager.TryPlaceBlock"/>.
    /// Visual feedback is intentionally bare — polish belongs to M2.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see README_M1.md.
    /// </summary>
    [RequireComponent(typeof(GameManagerBehaviour))]
    public class BlockDragController : MonoBehaviour
    {
        [SerializeField] private GridController gridController;
        [SerializeField] private Camera worldCamera;

        private GameManagerBehaviour _gameManagerBehaviour;
        private int _draggedSlotIndex = -1;
        private Transform _draggedVisual;

        private void Awake()
        {
            _gameManagerBehaviour = GetComponent<GameManagerBehaviour>();
            if (worldCamera == null) worldCamera = Camera.main;
        }

        /// <summary>Called by tray UI (M2) when the player starts dragging the block in <paramref name="traySlotIndex"/>.</summary>
        public void BeginDrag(int traySlotIndex, Transform visual)
        {
            _draggedSlotIndex = traySlotIndex;
            _draggedVisual = visual;
        }

        private void Update()
        {
            if (_draggedSlotIndex < 0 || _draggedVisual == null) return;

            Vector3 screenPosition = Input.mousePosition;
            if (Input.touchCount > 0) screenPosition = Input.GetTouch(0).position;

            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(
                new Vector3(screenPosition.x, screenPosition.y, -worldCamera.transform.position.z));
            worldPosition.z = 0f;
            _draggedVisual.position = worldPosition;

            bool released = Input.GetMouseButtonUp(0) ||
                             (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);
            if (released)
                EndDrag(worldPosition);
        }

        private void EndDrag(Vector3 worldPosition)
        {
            var origin = gridController.WorldToGrid(worldPosition);
            _gameManagerBehaviour.Game.TryPlaceBlock(_draggedSlotIndex, origin);

            _draggedSlotIndex = -1;
            _draggedVisual = null;
        }
    }
}
