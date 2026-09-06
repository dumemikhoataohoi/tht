using System;

namespace GemGrid.Core
{
    /// <summary>
    /// Lightweight integer 2D coordinate used by grid/gameplay logic. Deliberately
    /// UnityEngine-free (unlike Vector2Int) so gameplay logic can be compiled and unit
    /// tested outside the Unity Editor.
    /// </summary>
    [Serializable]
    public struct Int2 : IEquatable<Int2>
    {
        public int X;
        public int Y;

        public Int2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Int2 operator +(Int2 a, Int2 b) => new Int2(a.X + b.X, a.Y + b.Y);
        public static bool operator ==(Int2 a, Int2 b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Int2 a, Int2 b) => !(a == b);

        public bool Equals(Int2 other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is Int2 other && Equals(other);
        public override int GetHashCode() => (X * 397) ^ Y;
        public override string ToString() => $"({X}, {Y})";
    }
}
