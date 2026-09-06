namespace GemGrid.Configuration
{
    /// <summary>
    /// Supplies the current <see cref="BlockShapeLibrary"/> to gameplay (e.g. the spawner).
    /// Implemented by <see cref="BlockShapeSet"/> in production and by a fake in tests, so
    /// Gameplay never depends on ScriptableObject/UnityEngine directly.
    /// </summary>
    public interface IBlockShapeProvider
    {
        BlockShapeLibrary GetLibrary();
    }
}
