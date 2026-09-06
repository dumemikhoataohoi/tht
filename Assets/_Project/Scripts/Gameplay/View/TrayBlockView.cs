using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Sits on a single tray block visual. Starts the drag via
    /// <see cref="BlockDragController.BeginDrag"/> as soon as it is pressed/tapped.
    /// Requires a Collider2D on the same GameObject — <c>OnMouseDown</c> fires for both
    /// mouse clicks (Editor/desktop) and the primary touch (Android), so no separate
    /// touch-handling code is needed here.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see README_M1.md.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TrayBlockView : MonoBehaviour
    {
        public int SlotIndex { get; set; }
        public BlockDragController DragController { get; set; }

        private void OnMouseDown()
        {
            DragController.BeginDrag(SlotIndex, transform);
        }
    }
}
