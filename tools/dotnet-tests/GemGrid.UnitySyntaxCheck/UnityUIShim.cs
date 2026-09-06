// Extends UnityEngineShim.cs to cover the slice of UnityEngine.UI (uGUI) /
// UnityEngine.EventSystems API used by M2's Canvas-based UI
// (GameplayHud, GameOverScreen, MainMenuController, ButtonPunchFeedback,
// GemGridSceneSetup). Same purpose and same limitations as UnityEngineShim.cs —
// see the comment at the top of that file. This is NOT the real Unity UI system:
// it cannot verify actual layout, rendering, input routing, or Canvas behavior.
using UnityEngine.Events;

namespace UnityEngine
{
    public enum TextAnchor
    {
        UpperLeft, UpperCenter, UpperRight,
        MiddleLeft, MiddleCenter, MiddleRight,
        LowerLeft, LowerCenter, LowerRight
    }

    public enum RenderMode
    {
        ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace
    }

    public sealed class Canvas : Component
    {
        public RenderMode renderMode { get; set; }
    }

    public sealed class CanvasGroup : Behaviour
    {
        public float alpha { get; set; } = 1f;
        public bool interactable { get; set; } = true;
        public bool blocksRaycasts { get; set; } = true;
    }
}

namespace UnityEngine.UI
{
    public class Graphic : Component
    {
        public Color color { get; set; }
        public RectTransform rectTransform => GetComponent<RectTransform>();
    }

    public sealed class CanvasScaler : Component
    {
        public enum ScaleMode { ConstantPixelSize, ScaleWithScreenSize, ConstantPhysicalSize }
        public enum ScreenMatchMode { MatchWidthOrHeight, Expand, Shrink }

        public ScaleMode uiScaleMode { get; set; }
        public Vector2 referenceResolution { get; set; }
        public ScreenMatchMode screenMatchMode { get; set; }
        public float matchWidthOrHeight { get; set; }
    }

    public sealed class GraphicRaycaster : Component
    {
    }

    public sealed class Image : Graphic
    {
        public Sprite sprite { get; set; }
    }

    public sealed class Text : Graphic
    {
        public string text { get; set; }
        public Font font { get; set; }
        public int fontSize { get; set; }
        public TextAnchor alignment { get; set; }
    }

    public sealed class Button : Component
    {
        public UnityEvent onClick { get; } = new UnityEvent();
    }
}

namespace UnityEngine.EventSystems
{
    public sealed class EventSystem : Component
    {
    }

    public sealed class StandaloneInputModule : Component
    {
    }
}
