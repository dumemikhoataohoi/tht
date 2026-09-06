using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Resolves Unity's built-in legacy UI font across Editor versions: 2022.2+
    /// renamed the bundled font from "Arial.ttf" to "LegacyRuntime.ttf" (Arial was
    /// dropped for licensing reasons). Tries the new name first, falls back to the
    /// old one, so M2's placeholder UI text renders regardless of exact patch version.
    /// Real typography is a later polish pass, not M2 scope.
    /// </summary>
    public static class DefaultFont
    {
        private static Font _cached;

        public static Font Get()
        {
            if (_cached != null) return _cached;

            _cached = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_cached == null)
                _cached = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return _cached;
        }
    }
}
