using System;
using System.Collections.Generic;
using GemGrid.Configuration;

namespace GemGrid.Journey
{
    /// <summary>
    /// Wraps a base <see cref="BlockShapeLibrary"/> with re-weighted spawn odds favoring
    /// larger/more-awkward shapes as <see cref="ComplexityBias"/> increases — difficulty
    /// lever #2 (GAME_DESIGN_V2.md §6.2). A level using this only needs a differently
    /// biased <see cref="BlockShapeLibrary"/> passed to its own <c>BlockSpawner</c>; the
    /// spawner/library classes themselves are untouched.
    ///
    /// Every shape keeps at least weight 1 regardless of bias, so small/easy shapes never
    /// fully disappear — the explicit "no fake difficulty" rule in GAME_DESIGN_V2.md §7.
    /// </summary>
    public sealed class BiasedBlockShapeProvider : IBlockShapeProvider
    {
        private readonly BlockShapeLibrary _biasedLibrary;

        public BiasedBlockShapeProvider(BlockShapeLibrary baseLibrary, float complexityBias)
        {
            if (baseLibrary == null) throw new ArgumentNullException(nameof(baseLibrary));
            complexityBias = Math.Clamp(complexityBias, 0f, 1f);

            var biasedShapes = new List<BlockShapeDefinition>(baseLibrary.AllShapes.Count);
            foreach (var shape in baseLibrary.AllShapes)
            {
                int weight = Math.Max(1, (int)Math.Round(shape.SpawnWeight * (1f + complexityBias * (shape.CellCount - 1))));
                biasedShapes.Add(new BlockShapeDefinition(shape.Id, shape.Cells, weight));
            }

            _biasedLibrary = new BlockShapeLibrary(biasedShapes);
        }

        public BlockShapeLibrary GetLibrary() => _biasedLibrary;
    }
}
