using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Single source of truth for GemGrid's placeholder color system, shared by every
    /// view (Main Menu, Gameplay HUD, Grid, Tray, Game Over) so the whole game reads as
    /// one consistent visual product instead of per-file color literals. Original
    /// palette, generated for this project — not copied from any commercial game.
    /// </summary>
    public static class GemPalette
    {
        // Surfaces / backdrops.
        public static readonly Color Background = new Color(0.07f, 0.08f, 0.13f);
        public static readonly Color Surface = new Color(0.13f, 0.15f, 0.22f);
        public static readonly Color SurfaceAlt = new Color(0.17f, 0.19f, 0.27f);
        public static readonly Color TraySocket = new Color(0.11f, 0.12f, 0.18f);

        // Interactive accents.
        public static readonly Color Accent = new Color(0.35f, 0.70f, 0.95f);
        public static readonly Color AccentWarm = new Color(0.95f, 0.55f, 0.25f);
        public static readonly Color Neutral = new Color(0.50f, 0.50f, 0.56f);

        // Text.
        public static readonly Color TextPrimary = Color.white;
        public static readonly Color TextSecondary = new Color(0.72f, 0.75f, 0.85f);
        public static readonly Color TextCombo = new Color(1f, 0.85f, 0.30f);

        // Grid cell states.
        public static readonly Color CellEmpty = new Color(0.17f, 0.19f, 0.27f);
        public static readonly Color CellFilledFallback = new Color(0.25f, 0.85f, 0.65f);
        public static readonly Color ClearFlash = Color.white;

        // Original gem-like palette (not copied from any commercial game); the same
        // shape id always maps to the same color so blocks feel visually consistent.
        private static readonly Color[] Gems =
        {
            new Color(0.65f, 0.40f, 0.95f), // violet
            new Color(0.95f, 0.55f, 0.25f), // amber
            new Color(0.35f, 0.70f, 0.95f), // sapphire
            new Color(0.95f, 0.35f, 0.55f), // rose
            new Color(0.45f, 0.90f, 0.55f), // jade
        };

        public static Color ForShapeId(string shapeId)
        {
            if (string.IsNullOrEmpty(shapeId)) return Gems[0];
            int index = (shapeId.GetHashCode() & int.MaxValue) % Gems.Length;
            return Gems[index];
        }
    }
}
