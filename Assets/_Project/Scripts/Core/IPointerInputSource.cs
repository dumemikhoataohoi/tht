namespace GemGrid.Core
{
    /// <summary>Screen-space position of the primary pointer (mouse or first touch) this frame.</summary>
    public readonly struct PointerState
    {
        public readonly float X;
        public readonly float Y;
        public readonly bool Released;

        public PointerState(float x, float y, bool released)
        {
            X = x;
            Y = y;
            Released = released;
        }
    }

    /// <summary>
    /// Abstraction over "where is the primary pointer, and was it just released", so
    /// drag input logic (<see cref="BlockDragController"/>) does not call static
    /// UnityEngine.Input members directly — keeps that logic swappable/mockable without
    /// depending on which underlying input backend Unity uses.
    /// </summary>
    public interface IPointerInputSource
    {
        /// <returns>false if no pointer (mouse or touch) is currently available.</returns>
        bool TryGetPointer(out PointerState state);
    }
}
