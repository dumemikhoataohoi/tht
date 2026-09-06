using System;
using System.Collections.Generic;
using System.Linq;
using GemGrid.Core;

namespace GemGrid.Configuration
{
    /// <summary>
    /// Pure, UnityEngine-free view over a set of <see cref="BlockShapeDefinition"/>s with
    /// weighted random selection. Built from a <see cref="BlockShapeSet"/> ScriptableObject
    /// at runtime, or directly in tests.
    /// </summary>
    public class BlockShapeLibrary
    {
        private readonly List<BlockShapeDefinition> _shapes;
        private readonly int _totalWeight;

        public BlockShapeLibrary(IEnumerable<BlockShapeDefinition> shapes)
        {
            _shapes = shapes?.ToList() ?? new List<BlockShapeDefinition>();
            if (_shapes.Count == 0)
                throw new ArgumentException("BlockShapeLibrary requires at least one shape.", nameof(shapes));

            foreach (var shape in _shapes)
            {
                if (shape.SpawnWeight <= 0)
                    throw new ArgumentException($"Shape '{shape.Id}' must have a positive SpawnWeight.", nameof(shapes));
            }

            _totalWeight = _shapes.Sum(s => s.SpawnWeight);
        }

        public IReadOnlyList<BlockShapeDefinition> AllShapes => _shapes;

        public BlockShapeDefinition GetRandomShape(IRandomSource random)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));

            int roll = random.NextInt(0, _totalWeight);
            int cumulative = 0;
            foreach (var shape in _shapes)
            {
                cumulative += shape.SpawnWeight;
                if (roll < cumulative) return shape;
            }

            return _shapes[_shapes.Count - 1];
        }
    }
}
