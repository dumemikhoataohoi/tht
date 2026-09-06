using UnityEngine;

namespace GemGrid.Gameplay
{
    /// <summary>
    /// Generates a single reusable 1x1 white square sprite at runtime, so grid cells
    /// and tray blocks are visible immediately on Play without anyone having to
    /// manually assign a placeholder sprite asset in the Inspector. Real art belongs
    /// to M2 — this only exists so the M1.5 playable build has something to see.
    /// </summary>
    internal static class PlaceholderSprite
    {
        private static Sprite _white;

        public static Sprite White
        {
            get
            {
                if (_white == null)
                {
                    var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    texture.SetPixel(0, 0, Color.white);
                    texture.Apply();
                    _white = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
                }
                return _white;
            }
        }
    }
}
