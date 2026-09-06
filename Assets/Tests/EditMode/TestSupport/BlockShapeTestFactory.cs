using System.Linq;
using GemGrid.Configuration;
using GemGrid.Core;

namespace GemGrid.Tests.EditMode
{
    /// <summary>Small, deterministic shape fixtures for tests — not the real game's shape set.</summary>
    public static class BlockShapeTestFactory
    {
        public static BlockShapeDefinition SingleCell(int weight = 1) =>
            new BlockShapeDefinition("single", new[] { new Int2(0, 0) }, weight);

        public static BlockShapeDefinition Horizontal(int length, int weight = 1) =>
            new BlockShapeDefinition($"h{length}",
                Enumerable.Range(0, length).Select(i => new Int2(i, 0)).ToArray(), weight);

        public static BlockShapeDefinition Vertical(int length, int weight = 1) =>
            new BlockShapeDefinition($"v{length}",
                Enumerable.Range(0, length).Select(i => new Int2(0, i)).ToArray(), weight);
    }
}
