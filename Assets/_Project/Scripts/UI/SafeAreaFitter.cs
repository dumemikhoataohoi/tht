using UnityEngine;

namespace GemGrid.UI
{
    /// <summary>
    /// Shrinks its own RectTransform to <see cref="Screen.safeArea"/> so HUD/menu/panel
    /// content never sits under a notch, camera cutout, or rounded corner on real
    /// Android devices. Attach to a full-stretch child of the Canvas and parent the
    /// screen's actual content under it, instead of directly under the Canvas.
    ///
    /// NOT verified in the Unity Editor/Play Mode in this environment — see UNITY_SETUP.md.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        private void Awake() => _rect = GetComponent<RectTransform>();

        private void Start() => Apply();

        private void Update()
        {
            // Screen.safeArea can change on device rotation/resize; re-apply only when
            // it (or the screen size it's measured against) actually changes.
            if (_lastSafeArea != Screen.safeArea || _lastScreenSize.x != Screen.width || _lastScreenSize.y != Screen.height)
                Apply();
        }

        private void Apply()
        {
            _lastSafeArea = Screen.safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            if (_lastScreenSize.x <= 0 || _lastScreenSize.y <= 0) return;

            Vector2 anchorMin = _lastSafeArea.position;
            Vector2 anchorMax = _lastSafeArea.position + _lastSafeArea.size;
            anchorMin.x /= _lastScreenSize.x;
            anchorMin.y /= _lastScreenSize.y;
            anchorMax.x /= _lastScreenSize.x;
            anchorMax.y /= _lastScreenSize.y;

            _rect.anchorMin = anchorMin;
            _rect.anchorMax = anchorMax;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
