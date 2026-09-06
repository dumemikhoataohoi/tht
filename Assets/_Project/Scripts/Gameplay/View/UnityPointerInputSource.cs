using GemGrid.Core;
using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Default <see cref="IPointerInputSource"/> backed by Unity's classic Input Manager
    /// (<c>UnityEngine.Input</c>). This already supports real touch input on Android
    /// (<c>Input.touchCount</c>/<c>GetTouch</c>) without requiring the newer Input System
    /// package — kept as-is per PROJECT_PLAN.md's "simplest, lowest dependency" rule.
    /// Falls back to mouse position/button when there is no active touch, which is what
    /// makes mouse input work in the Editor and on desktop.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see README_M1.md.
    /// </summary>
    public class UnityPointerInputSource : IPointerInputSource
    {
        public bool TryGetPointer(out PointerState state)
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                bool released = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
                state = new PointerState(touch.position.x, touch.position.y, released);
                return true;
            }

            Vector3 mouse = Input.mousePosition;
            state = new PointerState(mouse.x, mouse.y, Input.GetMouseButtonUp(0));
            return true;
        }
    }
}
