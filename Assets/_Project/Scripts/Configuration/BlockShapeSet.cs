using System.Collections.Generic;
using UnityEngine;

namespace GemGrid.Configuration
{
    /// <summary>
    /// Data asset holding every block shape GemGrid can spawn, plus its spawn weight.
    /// Create via Assets ▸ Create ▸ GemGrid ▸ Configuration ▸ Block Shape Set, then author
    /// shapes in the Inspector. No shape data is hard-coded in gameplay scripts.
    /// See README_M1.md for a suggested starter shape list.
    /// </summary>
    [CreateAssetMenu(menuName = "GemGrid/Configuration/Block Shape Set", fileName = "BlockShapeSet")]
    public class BlockShapeSet : ScriptableObject, IBlockShapeProvider
    {
        [SerializeField]
        private List<BlockShapeDefinition> shapes = new List<BlockShapeDefinition>();

        public IReadOnlyList<BlockShapeDefinition> Shapes => shapes;

        public BlockShapeLibrary GetLibrary() => new BlockShapeLibrary(shapes);
    }
}
